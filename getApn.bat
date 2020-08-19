@echo off
cd platform-tools/
set "numeric=%2"

REM Get default APN ID
for /f "tokens=4 usebackq delims==, " %%F in (`adb -s %1 shell su -c "content query --uri content://telephony/carriers/preferapn --projection _id"`) do (
	set "default=%%F"
)

break>..\apn.csv
REM Get list of APNs for selected numeric
REM ID; Name; Numeric; APN
for /f "tokens=4,6,8 usebackq delims==, " %%F in (`adb -s %1 shell su -c "content query --uri content://telephony/carriers --projection _id:name:apn --where "numeric=%numeric%""`) do (
	if "%%F"=="%default%" (echo %%F;%%G;%numeric%;%%H;true>>..\apn.csv) else (echo %%F;%%G;%numeric%;%%H;false>>..\apn.csv)
) 
cd ..