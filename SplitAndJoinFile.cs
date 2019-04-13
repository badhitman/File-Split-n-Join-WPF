////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SplitterJoinerFileCore
{
    /// <summary>
    /// Направление чтение файла. Лево (к началу), право (к концу)
    /// </summary>
    public enum ReadingDirection { Left, Rifht };

    /// <summary>
    /// Класс работы с файлами. Нарезка, склейка ...
    /// </summary>
    public class SplitAndJoinFile
    {
        #region Событие, возникающее по мере выполнения процесса извлечения данных из файла
        public delegate void ProgressValueChangedHandler(int percentage);
        // Событие, возникающее по мере выполнения процесса извлечения данных из файла
        public event ProgressValueChangedHandler ProgressValueChange;
        #endregion

        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Поток файла результата
        /// </summary>
        protected FileStream fs_out;

        /// <summary>
        /// Исходный файл
        /// </summary>
        protected FileStream fs_read;

        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Режим кодировки данных
        /// </summary>
        public static Encoding EncodingMode { get; protected set; } = Encoding.UTF8;

        /// <summary>
        /// Текущая позиция в исходном файле
        /// </summary>
        public long Position
        {
            get => (fs_read is null || !fs_read.CanRead) ? -1 : fs_read.Position;
            set
            {
                if (fs_read is null || !fs_read.CanRead)
                    return;

                if (value < 0)
                    fs_read.Position = 0;
                else if (value > Length)
                    fs_read.Position = Length;
                else
                    fs_read.Position = value;
            }
        }

        /// <summary>
        /// Размер исходного файла
        /// </summary>
        public long Length => (fs_read is null || !fs_read.CanRead) ? -1 : fs_read.Length;

        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Преобразовать строку в шаблон данных для поиска
        /// </summary>
        /// <param name="search_string">Строка поиска</param>
        /// <param name="ignore_case">режим игнорирования регистра строки поиска</param>
        /// <returns>Шаблон данных дял поиска в текущей кодировке [EncodingMode]</returns>
        public static byte[][] StringToBytes(string search_string, bool ignore_case = false)
        {
            byte[][] result_bytes = new byte[search_string.Length][];
            string s;
            for (int i = 0; i < search_string.Length; i++)
            {
                s = search_string.Substring(i, 1);
                if (ignore_case)
                    result_bytes[i] = new byte[] { EncodingMode.GetBytes(s.ToLower())[0], EncodingMode.GetBytes(s.ToUpper())[0] };
                else
                    result_bytes[i] = new byte[] { EncodingMode.GetBytes(s)[0] };
            }
            return result_bytes;
        }

        /// <summary>
        /// Определить кодировку по имени
        /// </summary>
        /// <param name="encoding_name">Имя кодировки</param>
        /// <returns>Указатель кодировки, определённой по строке имени</returns>
        public static Encoding DetectEncoding(string encoding_name)
        {
            switch (encoding_name.ToLower())
            {
                case "utf8":
                    return Encoding.UTF8;
                case "ascii":
                    return Encoding.ASCII;
                case "unicode":
                    return Encoding.Unicode;
                case "bigendianunicode":
                    return Encoding.BigEndianUnicode;
                case "utf32":
                    return Encoding.UTF32;
                case "utf7":
                    return Encoding.UTF7;
                default:
                    return Encoding.Default;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="encoding">Режим кодировки файлов. Можно изменить в дальнейшем через метод SetEncoding</param>
        public SplitAndJoinFile(Encoding encoding) => SetEncoding(encoding);

        /// <summary>
        /// Установить кодировку работы с файлом. При попытке установить NULL -> установится по умолчанию: Encoding.UTF8
        /// </summary>
        public void SetEncoding(Encoding encoding = null)
        {
            if (encoding is null)
                EncodingMode = Encoding.UTF8;
            else
                EncodingMode = encoding;
        }
        public void SetEncoding(string string_encoding) => SetEncoding(DetectEncoding(string_encoding));

        /// <summary>
        /// Открыть для чтения файл
        /// </summary>
        /// <param name="path_file">Путь к файлу для чтения/обработки</param>
        /// <param name="PreDefBuferSize">Размер буфера чтения</param>
        public void OpenFile(string path_file)
        {
            CloseFile();

            fs_read = new FileStream(path_file, FileMode.Open, FileAccess.Read);
            fs_read.Lock(0, fs_read.Length);
        }

        /// <summary>
        /// Закрыть оригинальный файл (если открыт)
        /// </summary>
        public void CloseFile()
        {
            if (!(fs_read is null))
            {
                fs_read.Close();
                fs_read.Dispose();
                fs_read = null;
            }
        }

        #region read data in files
        /// <summary>
        /// Возвращает массив байт слева и справа от указанной точки указанного размера в байтах
        /// </summary>
        /// <param name="position">Точка от которой читать данные</param>
        /// <param name="size_area">Желаемый размер данных в каждом из направлений от точки (вначало и в конец)</param>
        public Dictionary<ReadingDirection, byte[]> ReadDataAboutPosition(long position, int size_area) => new Dictionary<ReadingDirection, byte[]>
        {
            { ReadingDirection.Left, ReadBytes(position - size_area, position) },
            { ReadingDirection.Rifht, ReadBytes(position, position + size_area) }
        };

        /// <summary>
        /// Читает и возвращает массив байт из файла. Если начальная точка больше или равна конечной точки, то возвращается пустой массив байт.
        /// </summary>
        /// <param name="StartPosition">Начальная точка чтения байт. Если меньше нуля, то читает с начала файла (с позиции 0). Если точка больше размера файла, то возвращается пустой массив байт.</param>
        /// <param name="EndPosition">Конечная точка чтения байт. Если точка больше размера фалйла, то читается до конца файла</param>
        /// <returns>Возвращает массив байт из файла с произвольной точки до произвольной точки</returns>
        public byte[] ReadBytes(long StartPosition, long EndPosition)
        {
            // Запоминаем позицию курсора в файле, что бы потом вернуть его на место
            long current_position_of_stream = Position;
            //
            if (StartPosition < 0)
                StartPosition = 0;

            if (EndPosition > Length)
                EndPosition = Length;

            if (Length < 1 || StartPosition >= EndPosition)
                return new byte[] { };

            byte[] returned_data = new byte[EndPosition - StartPosition];

            Position = StartPosition;

            for (int i = 0; i < returned_data.Length; i++)
                returned_data[i] = (byte)fs_read.ReadByte();
            //
            Position = current_position_of_stream;
            return returned_data;
        }
        #endregion

        #region FindDataAll by byte[][] or string
        public long[] FindDataAll(string searchdata, bool ignore_case, long StartPosition) => FindDataAll(StringToBytes(searchdata, ignore_case), StartPosition);
        public long[] FindDataAll(byte[][] bytes_search, long StartPosition)
        {
            List<long> indexes = new List<long>();

            long index_match = -1;
            while (true)
            {
                index_match = FindData(bytes_search, StartPosition);
                if (index_match < 0)
                    break;
                else
                {
                    indexes.Add(index_match);
                    StartPosition = index_match + bytes_search.Length;
                }
            }

            return indexes.ToArray();
        }
        #endregion

        #region FindData by byte[][] or string
        public long FindData(string searchdata, bool ignore_case, long StartPosition) => FindData(StringToBytes(searchdata, ignore_case), StartPosition);
        public long FindData(byte[][] bytes_search, long StartPosition)
        {
            if (StartPosition >= Length)
                return -1;

            // Запоминаем позицию курсора в файле, что бы потом вернуть его на место
            long original_position_of_stream = Position;

            // Сохраняем значение в отдельную переменную, чтобы не обращаться к ней в цикле
            long file_length = Length;

            long WorkingReadPosition = Position = StartPosition;

            long initial_match_index = 0;
            int synchronous_search_index = 0;
            int dataSearchLength = bytes_search.Length;
            while (WorkingReadPosition <= file_length)
            {
                if (bytes_search[synchronous_search_index].Contains((byte)fs_read.ReadByte()))
                {
                    if (synchronous_search_index == 0)
                        initial_match_index = WorkingReadPosition;

                    synchronous_search_index++;
                }
                else
                    synchronous_search_index = 0;

                // Если достигли конечного байта (искомого массивай байт) в котором последовательно сошлись все байты с прочитаными из файла - значит есть полное совпадение
                if (synchronous_search_index == dataSearchLength)
                {
                    // возвращаем позицию курсосра в файле на исходную позицию
                    Position = original_position_of_stream;
                    return initial_match_index;
                }

                WorkingReadPosition++;
            }

            // возвращаем позицию курсосра в файле на исходную позицию
            Position = original_position_of_stream;
            return -1;
        }
        #endregion

        /// <summary>
        /// Копирует часть данных из файла в новый файл с произвольной точки до произвольной точки
        /// </summary>
        /// <param name="PointStart">Точка, с которой нужно копировать данные в новый файл</param>
        /// <param name="PointEnd">Точка, до которой нужно копировать данные в новый файл</param>
        /// <param name="destFolder">Папка назначения нового файла</param>
        /// <param name="newFileName">Имя нового файла в папке назначения</param>
        public void ExtractData(long PointStart, long PointEnd, string destFolder, string newFileName)
        {
            /*long SizeData = PointEnd - PointStart;

            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            byte[] wdata = new byte[] { };
            fs_out = new FileStream(destFolder + "\\" + newFileName, FileMode.Create);
            bin_writ = new BinaryWriter(this.fs_out);

            int markerFlush = 0;
            long ActualPoint = 0;
            while (SizeData > 0)
            {

                if (SizeData > BuferSize)
                {
                    ActualPoint = PointStart + BuferSize;
                    bin_writ.Write(ReadBytes(PointStart, PointStart + BuferSize));
                    PointStart += BuferSize;
                    SizeData -= BuferSize;
                }
                else
                {
                    ActualPoint = PointStart + SizeData;
                    bin_writ.Write(ReadBytes(PointStart, PointStart + SizeData));
                    break;
                }

                markerFlush++;
                if (markerFlush >= 20)
                {
                    bin_writ.Flush();
                    fs_out.Flush();
                    markerFlush = 0;
                    ProgressValueChange?.Invoke(((int)(ActualPoint / (fs_in.Length / 100))));
                    //_win.Dispatcher.Invoke(new Action(() => ProgressBarValueChanged(((int)(ActualPoint / (fs_in.Length / 100))), _win)));
                }
            }
            ProgressValueChange?.Invoke(((int)(ActualPoint / (fs_in.Length / 100))));
            //_win.Dispatcher.Invoke(new Action(() => ProgressBarValueChanged(((int)(ActualPoint / (fs_in.Length / 100))), _win)));
            bin_writ.Close();
            fs_out.Close();*/
        }

        /// <summary>
        /// Из исходного файла создаёт новый(е) (не изменяя исходный) "нарезая" на указанные размеры файл(ы). Можно вырезать необходымый размер либо нарезать весь файл целиком на равные части.
        /// </summary>
        /// <param name="destFolder"></param>
        /// <param name="size"></param>
        /// <param name="repeat"></param>
        /// <param name="repeatEvery"></param>
        public void SplitFile(string destFolder, long size, bool repeat = false, int repeatEvery = 1)
        {
            int partFile = 0;
            long StartPosition = 0;
            long EndPosition = fs_read.Length;
            string strNewFileNames = Path.GetFileName(fs_read.Name);
            while (StartPosition + size * repeatEvery < EndPosition)
            {
                partFile++;
                ExtractData(StartPosition, StartPosition + size * repeatEvery, destFolder, strNewFileNames + ".part_" + partFile.ToString());
                StartPosition += size * repeatEvery;
                if (!repeat)
                {
                    ExtractData(StartPosition, EndPosition, destFolder, strNewFileNames + ".part_" + (partFile + 1).ToString());
                    return;
                }
            }
            if (StartPosition < EndPosition)
            {
                ExtractData(StartPosition, EndPosition, destFolder, strNewFileNames + ".part_" + (partFile + 1).ToString());
            }
        }

        /// <summary>
        /// Берёт исходный файл и создаёт "нарезку" файлов используя для разделителя byte[]
        /// </summary>
        /// <param name="destFolder">Папка назначения для новых файлов</param>
        /// <param name="textSplit">Текст по которому нужно делить файл</param>
        /// <param name="repeat">true - если следует нарезать весь файл. false - если нужно на вырезать одну часть файла</param>
        /// <param name="repeatEvery">Если нужно нарезать весь файл, то можно указать сколько вхождений искомой строки должно войти в одну партию файла</param>
        public void SplitFile(string destFolder, byte[][] dataSearch, bool repeat = false, int repeatEvery = 1)
        {
            int partFile = 0;
            long StartPosition = 0;
            //byte[] dataSearch = StringToByte(textSplit);
            int dataSearchLength = dataSearch.Length;
            long EndPosition = fs_read.Length;
            string strNewFileNames = Path.GetFileName(this.fs_read.Name);
            long entryPoint = FindData(dataSearch, 0);
            int countEvery = repeatEvery - 1;
            while (entryPoint + dataSearchLength < EndPosition)
            {
                if (countEvery > 0)
                {
                    countEvery--;
                    entryPoint = FindData(dataSearch, entryPoint + 1);
                    if (entryPoint < EndPosition)
                        continue;
                    else
                        break;
                }
                partFile++;
                ExtractData(StartPosition, entryPoint, destFolder, strNewFileNames + ".part_" + partFile.ToString());
                countEvery = repeatEvery;
                StartPosition = entryPoint;
            }
            if (StartPosition < entryPoint)
            {
                ExtractData(StartPosition, entryPoint, destFolder, strNewFileNames + ".part_" + (partFile + 1).ToString());
            }
        }

        /// <summary>
        /// Создать файл из нескольких "склеив" их последовательно один за одним
        /// </summary>
        /// <param name="files">Файлы, которые требуется "склеить"</param>
        /// <param name="fileNameSave">Путь/Имя нового файла, который получиться путём объединения других файлов</param>
        public static void JoinFiles(string[] files, string fileNameSave)
        {
            FileStream stream_w = new FileStream(fileNameSave, FileMode.Create);
            BinaryWriter binary_w = new BinaryWriter(stream_w);
            int BuferSize = 1024 * 64;
            //
            FileStream stream_r;
            BinaryReader binary_r;
            //
            foreach (string s in files)
            {
                stream_r = new FileStream(s, FileMode.Open, FileAccess.Read);
                binary_r = new BinaryReader(stream_r);
                long startPointRead = 0;
                long endPointRead = stream_r.Length;
                int markerFlush = 20;
                int SizePartData = 0;
                while (startPointRead < endPointRead)
                {
                    if (endPointRead - startPointRead > BuferSize)
                    {
                        SizePartData = BuferSize;
                    }
                    else
                    {
                        SizePartData = (int)(endPointRead - startPointRead);
                    }
                    byte[] bytesForWrite = new byte[SizePartData];
                    binary_r.Read(bytesForWrite, 0, SizePartData);
                    binary_w.Write(bytesForWrite);

                    markerFlush++;
                    if (markerFlush >= 200)
                    {
                        binary_w.Flush();
                        stream_w.Flush();
                        markerFlush = 0;
                    }
                    startPointRead += SizePartData;
                }
                binary_w.Flush();
                stream_w.Flush();
                stream_r.Close();
            }
            binary_w.Close();
            stream_w.Close();
        }
    }
}
