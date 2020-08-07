@echo off
cd platform-tools/

for /f "tokens=* usebackq" %%F in (`adb -s %1 shell settings get global device_name`) do (
	set "model=%%F"
)
for /f "usebackq" %%F in (`adb -s %1 shell getprop ro.build.version.release`) do (
	set "version=%%F"
)
adb -s %1 shell su -c "echo" && goto Root || goto NotRoot

:NotRoot
echo %model%;%version%;false>>../rootList.txt
goto End

:Root
echo %model%;%version%;true>>../rootList.txt

:End
cd..
