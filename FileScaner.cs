////////////////////////////////////////////////
// © https://github.com/badhitman 
////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    public class FileScaner : FileReader
    {
        /// <summary>
        /// Преобразовать строку в шаблон данных для поиска
        /// </summary>
        /// <param name="search_string">Строка поиска</param>
        /// <param name="ignore_case">режим игнорирования регистра строки поиска</param>
        /// <returns>Шаблон данных дял поиска в текущей кодировке [EncodingMode]</returns>
        public static byte[][] StringToSearchBytes(string search_string, bool ignore_case = false)
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

        #region FindDataAll by byte[][] or string
        public long[] FindDataAll(string searchdata, bool ignore_case, long StartPosition) => FindDataAll(StringToSearchBytes(searchdata, ignore_case), StartPosition);
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
        public long FindData(string searchdata, bool ignore_case, long StartPosition) => FindData(StringToSearchBytes(searchdata, ignore_case), StartPosition);
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
                if (bytes_search[synchronous_search_index].Contains((byte)FileReadStream.ReadByte()))
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
    }
}
