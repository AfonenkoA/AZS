С помощью Overpass API отправляем к Open Street Map запрос, описанный в Config\Query.txt. Запрашиваем json со всеми объектами типа fuel (заправка) на территории Беларуси.
Из полученного документа исключаем избыточные данные перечисленные в Config\ExceptedTags.txt.
Проверяем, имеются ли в наличие необходимых данных, перечисленных в Config\ExceptedTags.txt. Выводим результат.

Программа предоставляет интерфейс для вывода списка заправок на экран или в заданный файл.
run with:\n-c to show result in console;\n-f [filename] to write result in file;