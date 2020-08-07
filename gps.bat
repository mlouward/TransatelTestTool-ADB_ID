@echo off
cd platform-tools\
setlocal enabledelayedexpansion

REM Needs google maps installed
REM Wake up phone, activate data and open Google maps to get location
adb -s %1 shell input keyevent KEYCODE_WAKEUP & adb -s %1 shell input keyevent KEYCODE_MENU & adb -s %1 shell "svc data enable" & timeout 1 > nul
adb -s %1 shell "am start -n com.google.android.apps.maps/com.google.android.maps.MapsActivity" || goto ErrorMaps

timeout 5 > nul

adb -s %1 shell "dumpsys location | grep 'network: Location'">tmp.txt || goto Error
for /f "tokens=*" %%F in (tmp.txt) do (
	set "location=%%F" & goto next
)

:Next
echo %location%
goto End

:Error
echo Could not retrieve location.
goto End

:ErrorMaps
echo Could not find Google Maps. Make sure it is installed on the device.

:End
del tmp.txt
cd..
