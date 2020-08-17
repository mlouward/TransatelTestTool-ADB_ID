@echo off
cd platform-tools/
setlocal enabledelayedexpansion

for /f "tokens=1,2 usebackq delims=]" %%f in (`adb -s %1 shell "dumpsys isub | grep 'Id\[' | grep -Eo '[0-9].?$' | sed 's/\ //g'"`) do (
	set "id1=!id2!"
	set "id2=%%f"
)

for /f "tokens=* usebackq" %%F in (`adb -s %1 shell settings get global device_name`) do (
	set "model=%%F"
)
for /f "usebackq" %%F in (`adb -s %1 shell getprop ro.build.version.release`) do (
	set "version=%%F"
)
REM Test if phone is rooted or not
adb -s %1 shell su -c "echo" && goto Root || goto NotRoot

:NotRoot
echo %model%;%version%;false;!id1!;!id2!;%1>>../rootList.txt
goto End

:Root
echo %model%;%version%;true;!id1!;!id2!;%1>>../rootList.txt

:End
endlocal
cd..