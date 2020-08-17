REM @echo off
cd platform-tools\
setlocal enabledelayedexpansion

REM Set default Sim card to use for data
adb -s %1 shell svc data disable & adb -s %1 shell "su -c 'settings put global multi_sim_data_call %6 && am broadcast -a android.intent.action.SUB_DEFAULT_CHANGED'" & timeout 1 > nul

REM Disable WiFi and enable Data
adb -s %1 shell svc data enable & adb -s %1 shell svc wifi disable & timeout 5 > nul && echo [!date!-!time:~0,8!] Ping test initiated. (PHONE: %3, ADDRESS: %2, NB: %4, SIZE: %5) >>..\logs\pinglog.txt

adb -s %1 shell "ping -c %4 -s %5 %2">pingResults.txt 2>pingError.txt && goto Success || goto Error

:Success
for /F "delims=" %%a in (pingResults.txt) do (
   set "lastButOne=!lastLine!"
   set "lastLine=%%a"
)
echo [!date!-!time:~0,8!] %lastButOne%>>..\logs\pinglog.txt
echo [!date!-!time:~0,8!] %lastLine%>>..\logs\pinglog.txt
echo [!date!-!time:~0,8!] Ping test successful. (PHONE: %3, ADDRESS: %2, NB: %4, SIZE: %5) >>..\logs\pinglog.txt
goto End

:Error
for /F "delims=" %%a in (pingError.txt) do (
   set "error=%%a"
)
echo [!date!-!time:~0,8!] %error% (PHONE: %3, ADDRESS: %2, NB: %4, SIZE: %5) >>..\logs\pinglog.txt
echo [!date!-!time:~0,8!] Ping test unsuccessful (PHONE: %3, ADDRESS: %2, NB: %4, SIZE: %5) >>..\logs\pinglog.txt

:End
del pingResults.txt
del pingError.txt
endlocal
echo.>> ..\logs\pinglog.txt
cd..