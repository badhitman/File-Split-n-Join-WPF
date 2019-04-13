# File-Split-n-Join
Нарезка файлов
```C#
SplitAndJoinFile split = new SplitAndJoinFile(Encoding.UTF8);

split.OpenFile(@"C:\test.txt"); // test data in file  "000 ABC 111 Abc 222 AbC 333 aBC 444 abC 555abc666aBc777zxy888AAA999"
long index_detect_find_data = split.FindData("abc", true, 0); // 4
index_detect_find_data = split.FindData(SplitAndJoinFile.StringToBytes("abc"), 0); // 43
index_detect_find_data = split.FindData("ABC", ignore_case: false, StartPosition: 0); // 4
index_detect_find_data = split.FindData("AbC", ignore_case: true, StartPosition: 0); // 4

long[] indexes_detect_find_data = split.FindDataAll("abc", ignore_case: false, StartPosition: 0); // [43]
indexes_detect_find_data = split.FindDataAll("abc", ignore_case: true, StartPosition: 0); // [4, 12, 20, 28, 36, 43, 49]

split.CloseFile();
```
