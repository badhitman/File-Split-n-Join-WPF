////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System.IO;

namespace FileManager
{
    public class FileWriter : FileScaner
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
        protected FileStream FileWriteStream;

        /// <summary>
        /// Копирует часть данных из файла в новый файл с произвольной точки до произвольной точки
        /// </summary>
        /// <param name="PointStart">Точка, с которой нужно копировать данные в новый файл</param>
        /// <param name="PointEnd">Точка, до которой нужно копировать данные в новый файл</param>
        public void CopyData(long StartPosition, long EndPosition, string destFileName)
        {
            // Запоминаем позицию курсора в файле, что бы потом вернуть его на место
            long current_position_of_stream = Position;
            //
            if (StartPosition < 0)
                StartPosition = 0;

            if (EndPosition > Length)
                EndPosition = Length;

            if (Length < 1 || StartPosition >= EndPosition)
                return;

            if (!Directory.Exists(Path.GetDirectoryName(destFileName)))
                Directory.CreateDirectory(destFileName);

            FileWriteStream = new FileStream(destFileName, FileMode.Create);

            int markerFlush = 0;
            long ActualPoint = 0;
            Position = StartPosition;
            while (Position <= EndPosition)
            {
                FileWriteStream.WriteByte((byte)FileReadStream.ReadByte());

                markerFlush++;
                if (markerFlush >= 20)
                {
                    FileWriteStream.Flush();
                    markerFlush = 0;
                    ProgressValueChange?.Invoke(((int)(ActualPoint / (Length / 100))));
                }
            }
            ProgressValueChange?.Invoke(((int)(ActualPoint / (Length / 100))));

            FileWriteStream.Close();
            Position = current_position_of_stream;
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
