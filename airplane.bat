@echo off
cd platform-tools/
setlocal enabledelayedexpansion

REM Sets SELinux to Permisive (seems to be needed)
adb -s %1 shell su -c "setenforce 0" || echo [!date!-!time:~0,8!] Phone %3 is not rooted. >> ../logs/airplanelog.txt && goto End
adb -s %1 shell "settings put global airplane_mode_on 1" & adb -s %1 shell su -c "am broadcast -a android.intent.action.AIRPLANE_MODE">nul && echo [!date!-!time:~0,8!] Airplane Mode On (%3) >> ../logs/airplanelog.txt || echo [!date!-!time:~0,8!] Error: %3 not plugged in. >> ../logs/airplanelog.txt && goto End

timeout %2 >nul

adb -s %1 shell "settings put global airplane_mode_on 0" & adb -s %1 shell su -c "am broadcast -a android.intent.action.AIRPLANE_MODE">nul && echo [!date!-!time:~0,8!] Airplane Mode Off (%3) >> ../logs/airplanelog.txt

:End
endlocal
cd..