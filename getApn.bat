@echo off
cd platform-tools/
set "numeric=%2"

REM Get default APN ID
adb -s %1 shell su -c "content query --uri content://telephony/carriers/preferapn" --projection _id > default.txt
for /f "tokens=4 delims==, " %%F in (default.txt) do (
	set "default=%%F"
)
del default.txt

REM Get list of APNs for selected numeric
adb -s %1 shell su -c "content query --uri content://telephony/carriers --projection _id:name:apn --where "numeric=%numeric%"" > ../carriers.txt

break>..\apn.csv
REM ID; Name; Numeric; APN
for /f "tokens=4,6,8 delims==, " %%F in (../carriers.txt) do (
	if "%%F"=="%default%" (echo %%F;%%G;%numeric%;%%H;true>>..\apn.csv) else (echo %%F;%%G;%numeric%;%%H;false>>..\apn.csv)
) 
del ..\carriers.txt
cd ..