@echo off
cd platform-tools/
setlocal enabledelayedexpansion

REM For every phone, first get the sub ID for the 2 sim slots, then get the IMSI for both sims.
FOR /F "skip=1 tokens=1,2 USEBACKQ" %%a IN (`adb devices`) DO (
	IF "%%b"=="device" (
		for /f "tokens=1,2 usebackq delims=]" %%f in (`adb -s %%a shell "dumpsys isub | grep 'Id\[' | grep -Eo '[0-9].?$' | xargs | sed 's/\ //g'"`) do (
			set "id1=%%f"
			set "id2=%%g"
		)
		for /f "usebackq" %%f in (`adb -s "%%a" shell "service call iphonesubinfo 8 i32 !id1! | toybox cut -d \"'\" -f2 | toybox grep -Eo '[0-9]' | toybox xargs | toybox sed 's/\ //g'"`) do (
			echo %%f;%%a >> ..\imsiList.txt
			echo %%f;!id1! >> ..\imsiToSubId.txt
		)
		if defined id2 (
			for /f "usebackq" %%f in (`adb -s "%%a" shell "service call iphonesubinfo 8 i32 !id2! | toybox cut -d \"'\" -f2 | toybox grep -Eo '[0-9]' | toybox xargs | toybox sed 's/\ //g'"`) do (
				echo %%f;%%a >> ..\imsiList.txt
				echo %%f;!id2! >> ..\imsiToSubId.txt
			)
		)
	)
)
endlocal
cd ..
