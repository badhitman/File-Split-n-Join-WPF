////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace File_Split_and_Join
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
        /// <summary>
        /// Поток файла результата
        /// </summary>
        private FileStream fs_out;

        /// <summary>
        /// Исходный файл
        /// </summary>
        private FileStream fs_in;

        private BinaryReader bin_read;
        private BinaryWriter bin_writ;
        //
        private int _PreDefinedBuferSize;

        public static byte[] StreamToByte(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static byte[] StringToByte(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return StreamToByte(stream);
        }

        public static byte[] HexToByte(string s)
        {
            return s.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
        }

        public static string StringToHEX(string s)
        {
            return BitConverter.ToString(Encoding.Default.GetBytes(s));
        }

        ///////////////////////////////////////////////////////////////////////////////////

        public void OpenFile(string PathFile, int PreDefBuferSize = 8192)
        {
            fs_in = new FileStream(PathFile, FileMode.Open, FileAccess.Read);
            fs_in.Lock(0, fs_in.Length);
            _PreDefinedBuferSize = PreDefBuferSize;
        }

        public long Position
        {
            get { return fs_in.Position; }
            set
            {
                if (value < 0)
                    fs_in.Position = 0;
                else if (value > Length)
                    fs_in.Position = Length;
                else
                    fs_in.Position = value;
            }
        }

        public long Length
        {
            get { return fs_in.Length; }
        }

        /// <summary>
        /// Возвращает массив байт слева и справа от указанной точки указанного размера в байтах
        /// </summary>
        /// <param name="position">Точка от которой читать данные</param>
        /// <param name="sizeArea">Желаемый размер данных</param>
        /// <param name="encode">Кодировка в которой следует прочитать данные /System.Text.Encoding/ (Default, UTF8, ASCII, Unicode, BigEndianUnicode, UTF32, UTF7 или без указания кодировки)</param>
        /// <returns>Коллекция из двух значений Dictionary<ReadingDirection, byte[]></returns>
        public Dictionary<ReadingDirection, byte[]> ReadDataAboutPosition(long position, int sizeArea, string encode)
        {
            Dictionary<ReadingDirection, byte[]> returnedData = new Dictionary<ReadingDirection, byte[]> { };
            returnedData[ReadingDirection.Left] = readBytes(position - sizeArea, position, encode);
            returnedData[ReadingDirection.Rifht] = readBytes(position, position + sizeArea, encode);
            return returnedData;
        }

        /// <summary>
        /// Читает и возвращает массив байт из файла. Если начальная точка больше или равна конечной точки, то возвращается пустой массив байт.
        /// </summary>
        /// <param name="PointStart">Начальная точка чтения байт. Если меньше нуля, то читает с начала файла (с позиции 0). Если точка больше размера файла, то возвращается пустой массив байт.</param>
        /// <param name="PointEnd">Конечная точка чтения байт. Если точка больше размера фалйла, то читается до конца файла</param>
        /// <param name="encode">Кодировка (Не обязательно). Указывает в какой кодировке читать данные /System.Text.Encoding/ (Default, UTF8, ASCII, Unicode, BigEndianUnicode, UTF32, UTF7 или без указания кодировки)</param>
        /// <returns>Возвращает массив байт из файла с произвольной точки до произвольной точки</returns>
        public byte[] readBytes(long PointStart, long PointEnd, string encode = "")
        {
            long TruePosition = fs_in.Position;
            byte[] returned_data = new byte[] { };
            //
            if (PointStart < 0)
                PointStart = 0;
            if (PointEnd > fs_in.Length)
                PointEnd = fs_in.Length;
            if (PointStart > fs_in.Length || PointStart >= PointEnd)
                return returned_data;
            this.fs_in.Position = PointStart;
            returned_data = new byte[PointEnd - PointStart];
            switch (encode.ToLower())
            {
                case "default":
                    this.bin_read = new BinaryReader(this.fs_in, Encoding.Default);
                    break;
                case "utf8":
                    this.bin_read = new BinaryReader(this.fs_in, Encoding.UTF8);
                    break;
                case "ascii":
                    this.bin_read = new BinaryReader(this.fs_in, Encoding.ASCII);
                    break;
                case "unicode":
                    this.bin_read = new BinaryReader(this.fs_in, Encoding.Unicode);
                    break;
                case "bigendianunicode":
                    this.bin_read = new BinaryReader(this.fs_in, Encoding.BigEndianUnicode);
                    break;
                case "utf32":
                    this.bin_read = new BinaryReader(this.fs_in, Encoding.UTF32);
                    break;
                case "utf7":
                    this.bin_read = new BinaryReader(this.fs_in, Encoding.UTF7);
                    break;
                default:
                    this.bin_read = new BinaryReader(this.fs_in);
                    break;
            }
            bin_read.BaseStream.Position = PointStart;
            bin_read.Read(returned_data, 0, (int)(PointEnd - PointStart));
            fs_in.Position = TruePosition;
            return returned_data;
        }

        /// <summary>
        /// Копирует часть данных из файла в новый файл с произвольной точки до произвольной точки
        /// </summary>
        /// <param name="PointStart">Точка, с которой нужно копировать данные в новый файл</param>
        /// <param name="PointEnd">Точка, до которой нужно копировать данные в новый файл</param>
        /// <param name="destFolder">Папка назначения нового файла</param>
        /// <param name="newFileName">Имя нового файла в папке назначения</param>
        public void ExtractData(long PointStart, long PointEnd, string destFolder, string newFileName)
        {
            long SizeData = PointEnd - PointStart;
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            byte[] wdata = new byte[] { };
            this.fs_out = new FileStream(destFolder + "\\" + newFileName, FileMode.Create);
            bin_writ = new BinaryWriter(this.fs_out);

            int markerFlush = 0;
            long ActualPoint = 0;
            while (SizeData > 0)
            {

                if (SizeData > _PreDefinedBuferSize)
                {
                    ActualPoint = PointStart + _PreDefinedBuferSize;
                    bin_writ.Write(readBytes(PointStart, PointStart + _PreDefinedBuferSize));
                    PointStart += _PreDefinedBuferSize;
                    SizeData -= _PreDefinedBuferSize;
                }
                else
                {
                    ActualPoint = PointStart + SizeData;
                    bin_writ.Write(readBytes(PointStart, PointStart + SizeData));
                    break;
                }

                markerFlush++;
                if (markerFlush >= 20)
                {
                    bin_writ.Flush();
                    this.fs_out.Flush();
                    markerFlush = 0;
                    //_win.Dispatcher.Invoke(new Action(() => ProgressBarValueChanged(((int)(ActualPoint / (fs_in.Length / 100))), _win)));
                }
            }
            //_win.Dispatcher.Invoke(new Action(() => ProgressBarValueChanged(((int)(ActualPoint / (fs_in.Length / 100))), _win)));
            bin_writ.Close();
            this.fs_out.Close();
        }

        /// <summary>
        /// Найти в файле данные. Поиск производится с указанной точки и до конца
        /// </summary>
        /// <param name="dataSearch">Данные, которые нужно искать</param>
        /// <param name="PointStartSearch">Точка в файле с которой нужно начинать поиск</param>
        /// <returns>Позиция в файле, начиная с которой начинаются искомые данные</returns>
        public long FindData(byte[] dataSearch, long PointStartSearch)//, MainWindow my_win
        {
            long TruePosition = fs_in.Position;
            long myPointStartRead = PointStartSearch;
            long fs_inLength = fs_in.Length;
            fs_in.Position = myPointStartRead;
            int dataSearchLength = dataSearch.Length;
            while (myPointStartRead + dataSearchLength < fs_inLength && !dataSearch.SequenceEqual(readBytes(myPointStartRead, myPointStartRead + dataSearchLength)))
            {
                myPointStartRead++;
                if (myPointStartRead + dataSearchLength >= fs_inLength)
                    break;
            }
            if (dataSearch.SequenceEqual(readBytes(myPointStartRead, myPointStartRead + dataSearchLength)))
                return myPointStartRead;
            else
            {
                // System.Windows.MessageBox.Show("End of file", "End of file", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
            }
            fs_in.Position = TruePosition;
            return fs_inLength;
        }

        private void ProgressBarValueChanged(int percentage) // , FinalSplitWin _win
        {
            //_win.ProgressBarSplitFile.Value = percentage;
            //_win.TaskbarItemInfo.ProgressValue = (double)percentage / (double)100;
        }

        /// <summary>
        /// Из исходного файла создаёт новый(е) (не изменяя исходный) "нарезая" на указанные размеры файл(ы). Можно вырезать необходымый размер либо нарезать весь файл целиком на равные части.
        /// </summary>
        /// <param name="destFolder"></param>
        /// <param name="size"></param>
        /// <param name="repeat"></param>
        /// <param name="repeatEvery"></param>
        public void SplitFile(string destFolder, long size, bool repeat = false, int repeatEvery = 1)//FinalSplitWin _win, 
        {
            int partFile = 0;
            long StartPosition = 0;
            long EndPosition = fs_in.Length;
            string strNewFileNames = Path.GetFileName(this.fs_in.Name);
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
                //ExtractData(_win, StartPosition, EndPosition, destFolder, strNewFileNames + ".part_" + (partFile + 1).ToString());
            }
            //System.Windows.MessageBox.Show("End of file", "Done", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
        }

        /// <summary>
        /// Берёт исходный файл и создаёт "нарезку" файлов используя для разделителя строку
        /// </summary>
        /// <param name="destFolder">Папка назначения для новых файлов</param>
        /// <param name="textSplit">Текст по которому нужно делить файл</param>
        /// <param name="repeat">true - если следует нарезать весь файл. false - если нужно на вырезать одну часть файла</param>
        /// <param name="repeatEvery">Если нужно нарезать весь файл, то можно указать сколько вхождений искомой строки должно войти в одну партию файла</param>
        public void SplitFile(string destFolder, byte[] dataSearch, bool repeat = false, int repeatEvery = 1)//FinalSplitWin _win, 
        {
            int partFile = 0;
            long StartPosition = 0;
            //byte[] dataSearch = StringToByte(textSplit);
            int dataSearchLength = dataSearch.Length;
            long EndPosition = fs_in.Length;
            string strNewFileNames = Path.GetFileName(this.fs_in.Name);
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
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="fileNameSave"></param>
        public void JoinFiles(string[] files, string fileNameSave)
        {
            FileStream stream_w = new FileStream(fileNameSave, FileMode.Create);
            BinaryWriter binary_w = new BinaryWriter(stream_w);
            this._PreDefinedBuferSize = 1024 * 64;
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
                    if (endPointRead - startPointRead > _PreDefinedBuferSize)
                    {
                        SizePartData = _PreDefinedBuferSize;
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
