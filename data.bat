@echo off
cd platform-tools\
setlocal enabledelayedexpansion

REM Set default Sim card to use for data
adb -s %1 shell svc data disable & adb -s %1 shell "settings put global multi_sim_data_call %5 && am broadcast -a android.intent.action.SUB_DEFAULT_CHANGED" & timeout 1 > nul

REM Disable WiFi and enable Data
adb -s %1 shell svc data enable & adb -s %1 shell svc wifi disable & timeout 5 > nul 

echo [%time:~0,8%] Beginning Data routine.
REM Turn on screen and unlock if necessary. Disables WiFi to make sure we use mobile data.
adb -s %1 shell input keyevent KEYCODE_WAKEUP & adb -s %1 shell input swipe 100 800 100 0 1000 & adb -s %1 shell input keyevent KEYCODE_MENU & timeout 1 > nul && echo [!date!-!time:~0,8!] Data test initiated. (PHONE: %3, URL: %2, NB: %4) >>..\logs\datalog.txt
set /a "nb=%4"

for /l %%F in (1,1,%nb%) do (
	adb -s %1 shell am start -a android.intent.action.VIEW -d "%2" >nul && echo [!date!-!time:~0,8!] Browser opened. ^(PHONE: %3, URL: %2, NB: %%F^) >>..\logs\datalog.txt || goto Error
	REM Delay 2 seconds between requests
	timeout 2 >nul
)
REM Turn off screen
adb -s %1 shell input keyevent KEYCODE_WAKEUP & adb -s %1 shell input keyevent KEYCODE_POWER
echo [%time:~0,8%] Data process succesful.
echo [!date!-!time:~0,8!] Data test successful. (PHONE: %3, URL: %2, NB: %4) >>..\logs\datalog.txt
goto End

:Error
echo Couldn't open the web page. Make sure the device is connected
echo [!date!-!time:~0,8!] Could not open browser. (PHONE: %3, URL: %2) >>..\logs\datalog.txt

:End
endlocal
echo.>> ..\logs\datalog.txt
cd..