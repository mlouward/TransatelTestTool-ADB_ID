@echo off
cd platform-tools/
setlocal enabledelayedexpansion

REM For every phone, first get the sub ID for the 2 sim slots, then get the IMSI for both sims.
FOR /F "skip=1 tokens=1,2 USEBACKQ" %%a IN (`adb devices`) DO (
	IF "%%b"=="device" (
		for /f "tokens=2 usebackq delims==" %%F in (`adb -s "%%a" shell "dumpsys isub | grep 'sSlotIndexToSubId\['"`) do (
			set "id1=!id2!"
			set "id2=%%F"
		)
		for /f "usebackq" %%f in (`adb -s "%%a" shell "service call iphonesubinfo 8 i32 !id1! | toybox cut -d \"'\" -f2 | toybox grep -Eo '[0-9]' | toybox xargs | toybox sed 's/\ //g'"`) do (
			echo %%f;%%a >> ..\imsiList.txt
		)
		for /f "usebackq" %%f in (`adb -s "%%a" shell "service call iphonesubinfo 8 i32 !id2! | toybox cut -d \"'\" -f2 | toybox grep -Eo '[0-9]' | toybox xargs | toybox sed 's/\ //g'"`) do (
			echo %%f;%%a >> ..\imsiList.txt
		)
	)
)

endlocal
cd ..
