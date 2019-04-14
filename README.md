# File-Split-n-Join
Нарезка файлов
```C#
// Конструктору передаём режим кодировки файлов. Можно изменить в дальнейшем через метод SetEncoding
SplitAndJoinFile split = new SplitAndJoinFile(Encoding.UTF8);
// открываем файл для чтения. В примере файл лежит по адресу [C:\test.txt]
// В файл записан текст [000 ABC 111 Abc 222 AbC 333 aBC 444 abC 555abc666aBc777zxy888AAA999] без скобок
split.OpenFile(@"C:\test.txt");
//////////////////////////////////////////////////////
// Поиск вхождений по одному

// поиск первого вхождения строки в режиме [игнорировать регистр].
long index_detect_find_data = split.FindData("abc", true, 0); // Результат поиска - 4

// поиск первого вхождения строки в режиме [учитывать регистр].
index_detect_find_data = split.FindData(SplitAndJoinFile.StringToBytes("abc"), 0); // Результат поиска - 43

// поиск первого вхождения строки в режиме [учитывать регистр].
index_detect_find_data = split.FindData("ABC", ignore_case: false, StartPosition: 0); // Результат поиска - 4

// поиск первого вхождения строки в режиме [игнорировать регистр].
index_detect_find_data = split.FindData("AbC", ignore_case: true, StartPosition: 0); // Результат поиска - 4

//////////////////////////////////////////////////////
// Поиск сразу всех вхождений. Полное сканирование файла

// поиск всех вхождений строки в режиме [Учитывать регистр]. 
long[] indexes_detect_find_data = split.FindDataAll("abc", ignore_case: false, StartPosition: 0); // Результат поиска: массив индексов - [43]

// поиск всех вхождений строки в режиме [игнорировать регистр].
indexes_detect_find_data = split.FindDataAll("abc", ignore_case: true, StartPosition: 0); // Результат поиска: массив индексов - [4, 12, 20, 28, 36, 43, 49]

split.OpenFile(@"C:\Users\user\AppData\Local\Temp\2019-04-10_13-38-35_863a4c2600544c65b16f26199339a282.tmp.http.post");
indexes_detect_find_data = split.FindDataAll("-----------------------------180702200020546", false, 0);
// Не забываем закрыьб файл
split.CloseFile();
```
