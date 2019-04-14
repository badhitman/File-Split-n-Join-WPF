////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManager
{
    /// <summary>
    /// Направление чтение файла. Лево (к началу), право (к концу)
    /// </summary>
    public enum ReadingDirection { Left, Rifht };

    /// <summary>
    /// Класс работы с файлами. Нарезка, склейка ...
    /// </summary>
    public class FileReader
    {
        /// <summary>
        /// Исходный файл
        /// </summary>
        protected FileStream FileReadStream;
        public static byte[][] HexToByte(string s)
        {
            byte[] original_bytes = s.Split('-').Select(b => Convert.ToByte(b, 16)).ToArray();
            byte[][] search_data = new byte[original_bytes.Length][];
            int original_bytes_length = original_bytes.Length;

            for (int i = 0; i < original_bytes_length; i++)
                search_data[i] = new byte[] { original_bytes[i] };

            return search_data;
        }
        public static string BytesToHEX(byte[] bytes) => BitConverter.ToString(bytes);
        public static string StringToHEX(string original_string) => BytesToHEX(EncodingMode.GetBytes(original_string));

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
                returned_data[i] = (byte)FileReadStream.ReadByte();
            //
            Position = current_position_of_stream;
            return returned_data;
        }
        #endregion

        /// <summary>
        /// Текущая позиция в исходном файле
        /// </summary>
        public long Position
        {
            get => (FileReadStream is null || !FileReadStream.CanRead) ? -1 : FileReadStream.Position;
            set
            {
                if (FileReadStream is null || !FileReadStream.CanRead)
                    return;

                if (value < 0)
                    FileReadStream.Position = 0;
                else if (value > Length)
                    FileReadStream.Position = Length;
                else
                    FileReadStream.Position = value;
            }
        }

        /// <summary>
        /// Размер исходного файла
        /// </summary>
        public long Length => (FileReadStream is null || !FileReadStream.CanRead) ? -1 : FileReadStream.Length;


        /// <summary>
        /// Открыть для чтения файл
        /// </summary>
        /// <param name="path_file">Путь к файлу для чтения/обработки</param>
        /// <param name="PreDefBuferSize">Размер буфера чтения</param>
        public void OpenFile(string path_file)
        {
            CloseFile();
            //
            FileReadStream = new FileStream(path_file, FileMode.Open, FileAccess.Read);
            FileReadStream.Lock(0, Length);
        }

        /// <summary>
        /// Закрыть оригинальный файл (если открыт)
        /// </summary>
        public void CloseFile()
        {
            if (!(FileReadStream is null))
            {
                FileReadStream.Close();
                FileReadStream.Dispose();
                FileReadStream = null;
            }
        }

        /// <summary>
        /// Режим кодировки данных
        /// </summary>
        public static Encoding EncodingMode { get; protected set; } = Encoding.UTF8;

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
    }
}
