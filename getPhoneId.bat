@echo off

cd platform-tools\
FOR /F "tokens=1,2 USEBACKQ" %%a IN (`adb devices`) DO (
	IF "%%b"=="device" (
		for /f "usebackq" %%f in (`adb -s "%%a" shell "service call iphonesubinfo 8 i32 1 | toybox cut -d \"'\" -f2 | toybox grep -Eo '[0-9]' | toybox xargs | toybox sed 's/\ //g'"`) do (
			echo %%f;%%a >> ..\imsiList.txt
		)
		for /f "usebackq" %%f in (`adb -s "%%a" shell "service call iphonesubinfo 8 i32 2 | toybox cut -d \"'\" -f2 | toybox grep -Eo '[0-9]' | toybox xargs | toybox sed 's/\ //g'"`) do (
			echo %%f;%%a >> ..\imsiList.txt
		)
	)
)
cd ..
