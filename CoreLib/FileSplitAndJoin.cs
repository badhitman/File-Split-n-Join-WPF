////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System.IO;

namespace FileManager
{
    public class FileSplitAndJoin : FileWriter
    {
        /// <summary>
        /// Из исходного файла создаёт новый(е) (не изменяя исходный) "нарезая" на указанные размеры файл(ы).
        /// </summary>
        public void SplitFile(string destination_folder, long size, int dimension_group = 1)
        {
            if (size < 1 || Length < size)
                return;

            int part_file = 0;
            long StartPosition = 0;
            long EndPosition = FileReadStream.Length;
            string tmpl_new_file_names = Path.GetFileName(FileReadStream.Name);
            while (StartPosition + size * dimension_group < EndPosition)
            {
                part_file++;
                CopyData(StartPosition, StartPosition + size * dimension_group, Path.Combine(destination_folder, tmpl_new_file_names + ".part_" + part_file.ToString()));
                StartPosition += size * dimension_group;
            }

            if (StartPosition < EndPosition)
                CopyData(StartPosition, EndPosition, Path.Combine(destination_folder, tmpl_new_file_names + ".part_" + (part_file + 1).ToString()));
        }

        /// <summary>
        /// Берёт исходный файл и создаёт "нарезку" файлов используя для разделителя строку или byte[].
        /// Будут созданы новые файлы, которые будут начинаться с искомых данных длинной до начала следующего вхождения (или до конца, если такого вхождения далее нет).
        /// Если первое вхождение не в самом начале файла, то предварительно будет создан "нулевой" файл с первого байта исходного файла до первого вхождения
        /// </summary>
        /// <param name="destination_folder">Папка назначения для новых файлов</param>
        /// <param name="data_search">Образец данных по которому нужно делить файл</param>
        /// <param name="dimension_group">Сколько вхождений искомой строки должно войти в одну партию файла</param>
        public void SplitFile(string destination_folder, string data_search, int dimension_group = 1) => SplitFile(destination_folder, StringToSearchBytes(data_search), dimension_group);
        public void SplitFile(string destination_folder, byte[][] data_search, int dimension_group = 1)
        {
            if (data_search.Length == 0 || Length < data_search.Length)
                return;

            long[] entry_points = FindDataAll(data_search, 0);
            if (entry_points.Length == 0 || (entry_points.Length == 1 && entry_points[0] == 0))
                return;

            if (dimension_group <= 0)
                dimension_group = 1;

            int data_search_length = data_search.Length;

            string tmpl_new_file_names = Path.GetFileName(FileReadStream.Name) + ".split.part_";

            if (entry_points[0] > 0)
                CopyData(0, entry_points[0], tmpl_new_file_names + "0");

            int operative_dimension_group = 0;
            int part_file = 1;
            int entry_points_length = entry_points.Length;
            long start_position_copy_data = -1;
            foreach (long point in entry_points)
            {
                if (operative_dimension_group <= 1)
                {
                    start_position_copy_data = start_position_copy_data < 0 ? point : start_position_copy_data;
                }

                if (operative_dimension_group == dimension_group)
                {
                    CopyData(start_position_copy_data, point, Path.Combine(destination_folder, tmpl_new_file_names + part_file.ToString()));
                    start_position_copy_data = point;

                    operative_dimension_group = 0;
                    part_file++;
                }

                operative_dimension_group++;
            }
            
            CopyData(start_position_copy_data, Length, Path.Combine(destination_folder, tmpl_new_file_names + part_file.ToString()));
        }
    }
}
