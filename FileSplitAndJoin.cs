////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System.IO;

namespace FileManager
{
    public class FileSplitAndJoin : FileWriter
    {
        /// <summary>
        /// Из исходного файла создаёт новый(е) (не изменяя исходный) "нарезая" на указанные размеры файл(ы). Можно вырезать необходымый размер либо нарезать весь файл целиком на равные части.
        /// </summary>
        public void SplitFile(string destFolder, long size, bool repeat = false, int repeatEvery = 1)
        {
            int partFile = 0;
            long StartPosition = 0;
            long EndPosition = FileReadStream.Length;
            string strNewFileNames = Path.GetFileName(FileReadStream.Name);
            while (StartPosition + size * repeatEvery < EndPosition)
            {
                partFile++;
                CopyData(StartPosition, StartPosition + size * repeatEvery, destFolder, strNewFileNames + ".part_" + partFile.ToString());
                StartPosition += size * repeatEvery;
                if (!repeat)
                {
                    CopyData(StartPosition, EndPosition, destFolder, strNewFileNames + ".part_" + (partFile + 1).ToString());
                    return;
                }
            }
            if (StartPosition < EndPosition)
            {
                CopyData(StartPosition, EndPosition, destFolder, strNewFileNames + ".part_" + (partFile + 1).ToString());
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
            long EndPosition = Length;
            string strNewFileNames = Path.GetFileName(this.FileReadStream.Name);
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
                CopyData(StartPosition, entryPoint, destFolder, strNewFileNames + ".part_" + partFile.ToString());
                countEvery = repeatEvery;
                StartPosition = entryPoint;
            }
            if (StartPosition < entryPoint)
            {
                CopyData(StartPosition, entryPoint, destFolder, strNewFileNames + ".part_" + (partFile + 1).ToString());
            }
        }
    }
}
