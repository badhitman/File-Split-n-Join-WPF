using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_Split_and_Join_old
{
    public enum ReadingDirection { Left, Rifht };
    public enum FillMethod { Before, After };

    public class FM
    {
        private FileStream fs_out;
        private FileStream fs_in;
        private BinaryReader br;
        private BinaryWriter bw;
        //
        long numPart = 1;
        private int PreDefinedCacheSize = 8192;

        public FM(string PathFile, int inCacheSize)
        {
            fs_in = new FileStream(PathFile, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Устанавливает/возвращает текущую позицию чтения байтов в файле.
        /// Sets/returns the current position of the read bytes in the file.
        /// </summary>
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

        /// <summary>
        /// Длинна файла
        /// </summary>
        public long Length
        {
            get { return fs_in.Length; }
        }

        public byte[] readBytes(ReadingDirection Direction, string encode = null, int count = -1, bool FreezePosition = true)
        {
            byte[] returned_data = new byte[] { };
            if (Direction == ReadingDirection.Left && this.Position < count)
                count = (int)this.Position;
            if (Direction == ReadingDirection.Rifht && this.Length - this.Position < count)
                count = (int)(this.Length - this.Position);
            //
            if (count < 0)
            {
                throw new System.ArgumentException("The size of the requested data is not correct.");
            }
            long is_position = this.Position;
            long tmp_position = this.Position;

            while (tmp_position > Length && tmp_position > 0)
            {
                count--;
                tmp_position--;
            }
            count = (int)Math.Min((long)(this.fs_in.Length - tmp_position), count);
            if (count > 0)
            {
                returned_data = new byte[count];
                if (Direction == ReadingDirection.Left)
                {
                    if (this.Position - count >= 0)
                        this.Position = this.fs_in.Position - count;
                    else
                    {
                        this.Position = 0;
                    }
                }
                switch (encode)
                {
                    case "Default":
                        this.br = new BinaryReader(this.fs_in, System.Text.Encoding.Default);
                        break;
                    case "UTF8":
                        this.br = new BinaryReader(this.fs_in, System.Text.Encoding.UTF8);
                        break;
                    case "ASCII":
                        this.br = new BinaryReader(this.fs_in, System.Text.Encoding.ASCII);
                        break;
                    case "Unicode":
                        this.br = new BinaryReader(this.fs_in, System.Text.Encoding.Unicode);
                        break;
                    case "BigEndianUnicode":
                        this.br = new BinaryReader(this.fs_in, System.Text.Encoding.BigEndianUnicode);
                        break;
                    case "UTF32":
                        this.br = new BinaryReader(this.fs_in, System.Text.Encoding.UTF32);
                        break;
                    case "UTF7":
                        this.br = new BinaryReader(this.fs_in, System.Text.Encoding.UTF7);
                        break;
                    default:
                        this.br = new BinaryReader(this.fs_in);
                        break;
                }

                br.BaseStream.Position = this.Position;
                br.Read(returned_data, 0, count);
            }
            if (FreezePosition)
                this.Position = is_position;
            return returned_data;
        }

        public static byte[] ReadFully(Stream input)
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

        public static byte[] GetBytes(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return ReadFully(stream);
        }

        public long FindData(byte[] dataSearch, bool DeltaResiult = true)
        {
            long oldFilePointRead = g.FileManager.Position;
            if (oldFilePointRead != 0 && oldFilePointRead != g.FileManager.Length)
                g.FileManager.Position++;
            int dataSearchLength = dataSearch.Length;
            if (g.FileManager.Position >= g.FileManager.Length - 1 - dataSearchLength)
            {
                System.Windows.MessageBox.Show("End of file", "End of file", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
                g.FileManager.Position = oldFilePointRead;
                return g.FileManager.Length - 1;
            }
            while (g.FileManager.Position + dataSearch.Length < g.FileManager.Length && !dataSearch.SequenceEqual(this.readBytes(ReadingDirection.Rifht, null, dataSearch.Length, true)))
            {
                if (g.FileManager.Position == g.FileManager.Length)
                {
                    long retVal = DeltaResiult ? g.FileManager.Length - oldFilePointRead : g.FileManager.Length;
                    g.FileManager.Position = oldFilePointRead;
                    return retVal;
                }
                g.FileManager.Position++;
            }
            if (dataSearch.SequenceEqual(this.readBytes(ReadingDirection.Rifht, null, dataSearch.Length, true)))
            {
                long retVal = DeltaResiult ? g.FileManager.Position - oldFilePointRead : g.FileManager.Position;
                g.FileManager.Position = oldFilePointRead;
                return retVal;
            }
            else
            {
                System.Windows.MessageBox.Show("End of file", "End of file", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Asterisk);
                long retVal = DeltaResiult ? g.FileManager.Length - oldFilePointRead : g.FileManager.Length;
                g.FileManager.Position = oldFilePointRead;
                return retVal;
            }
        }

        public void Split(string destFolder, long SplitSize, bool repeat = false, int wathRepeat = 1, bool ResetPoint = true)
        {
            
            long OldPointFile = fs_in.Position;
            if (ResetPoint)
            {
                numPart = 1;
                fs_in.Position = 0;
            }
            else
            {
                numPart++;
            }

            string strDirectory = destFolder + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(this.fs_in.Name);
            int attemptS = 5;
            while (!Directory.Exists(strDirectory) && !Directory.CreateDirectory(strDirectory).Exists && attemptS > 0)
            {
                attemptS--;
            }

            if (!Directory.Exists(strDirectory))
            {
                string s = "Failed to create a folder: {" + strDirectory + "}";
                System.Windows.MessageBox.Show(s);
                throw new System.InvalidOperationException(s);
            }

            string strNewFileNames = Path.GetFileName(this.fs_in.Name);
            byte[] wdata = new byte[] { };
            BinaryReader br = new BinaryReader(this.fs_in);
            this.fs_out = new FileStream(strDirectory + "\\" + strNewFileNames + ".part_" + numPart.ToString(), FileMode.Create);
            bw = new BinaryWriter(this.fs_out);

            br = new BinaryReader(this.fs_in);
            int tmpPreDefinedCacheSize;
            int markerFlush = 0;
            while (fs_in.Position < fs_in.Length - 1)
            {
                if (fs_in.Position + PreDefinedCacheSize < fs_in.Length - 1 && fs_out.Length + PreDefinedCacheSize <= SplitSize * wathRepeat)
                    tmpPreDefinedCacheSize = PreDefinedCacheSize;
                else if (fs_in.Position + PreDefinedCacheSize < fs_in.Length - 1 && fs_out.Length + PreDefinedCacheSize > SplitSize * wathRepeat)
                    tmpPreDefinedCacheSize = (int)(SplitSize * wathRepeat - fs_out.Length);
                else
                    tmpPreDefinedCacheSize = (int)Math.Min((fs_in.Length - fs_in.Position), SplitSize);
                wdata = new byte[tmpPreDefinedCacheSize];
                br.Read(wdata, 0, tmpPreDefinedCacheSize);
                bw.Write(wdata);
                if (fs_out.Length == SplitSize * wathRepeat)
                {
                    fs_out.Flush();
                    fs_out.Close();
                    markerFlush = 0;
                    if (repeat)
                    {
                        numPart++;
                        this.fs_out = new FileStream(strDirectory + "\\" + strNewFileNames + ".part_" + numPart.ToString(), FileMode.Create);
                        bw = new BinaryWriter(this.fs_out);
                    }
                    else
                        break;
                }
                markerFlush++;
                if (markerFlush >= 20)
                {
                    bw.Flush();
                    this.fs_out.Flush();
                    markerFlush = 0;
                }
            }

            bw.Close();
            fs_out.Close();
            if (ResetPoint)
                g.FileManager.Position = OldPointFile;
            if (ResetPoint)
                g.OpenFolder(strDirectory);
        }

        public void Split(string destFolder, string SplitText, bool repeat = false, int wathRepeat = 1)
        {
            numPart = 0;
            byte[] dataSearch = GetBytes(SplitText);
            int dataSearchLength = dataSearch.Length;
            int jobWathRepeat = wathRepeat;
            long OldPointFile = fs_in.Position;
            fs_in.Position = 0;
            long searchPoint = 0;
            long summSearchPoint = 0;
            long newPositionPointFile = 0;
            while (fs_in.Position < fs_in.Length - 1 - dataSearchLength)
            {
                searchPoint = FindData(dataSearch);
                newPositionPointFile += searchPoint;
                summSearchPoint += searchPoint;
                //if (searchPoint == fs_in.Length)
                //    return;
                jobWathRepeat--;
                if (jobWathRepeat > 0)
                {
                    fs_in.Position += searchPoint;
                    if (fs_in.Position + searchPoint < fs_in.Length)
                        continue;
                }
                jobWathRepeat = wathRepeat;
                
                fs_in.Position = OldPointFile;
                Split(destFolder, summSearchPoint, false, 1, false);
                fs_in.Position = newPositionPointFile;
                OldPointFile = newPositionPointFile;
                searchPoint = 0;
                summSearchPoint = 0;
            }

        }
    }
}
