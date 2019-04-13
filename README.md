# File-Split-n-Join
Нарезка файлов
```C#
SplitAndJoinFile split = new SplitAndJoinFile(Encoding.UTF8);
// открываем файл для чтения. В примере файл лежит по адресу [C:\test.txt]
// В файл записан текст [000 ABC 111 Abc 222 AbC 333 aBC 444 abC 555abc666aBc777zxy888AAA999] без скобок
split.OpenFile(@"C:\test.txt");
// Поиск вхождений по одному
long index_detect_find_data = split.FindData("abc", true, 0); // поиск первого вхождения строки в режиме [игнорировать регистр]. Результат поиска - 4
index_detect_find_data = split.FindData(SplitAndJoinFile.StringToBytes("abc"), 0); // поиск первого вхождения строки в режиме [учитывать регистр]. Результат поиска - 43
index_detect_find_data = split.FindData("ABC", ignore_case: false, StartPosition: 0); // поиск первого вхождения строки в режиме [учитывать регистр]. Результат поиска - 4
index_detect_find_data = split.FindData("AbC", ignore_case: true, StartPosition: 0); // поиск первого вхождения строки в режиме [игнорировать регистр]. Результат поиска - 4

// Поиск сразу всех вхождений. Полное сканирование файла
long[] indexes_detect_find_data = split.FindDataAll("abc", ignore_case: false, StartPosition: 0); // поиск всех вхождений строки в режиме [Учитывать регистр]. Результат поиска: массив индексов - [43]
indexes_detect_find_data = split.FindDataAll("abc", ignore_case: true, StartPosition: 0); // поиск всех вхождений строки в режиме [игнорировать регистр]. Результат поиска: массив индексов - [4, 12, 20, 28, 36, 43, 49]
// Не забываем закрыьб файл
split.CloseFile();
```
