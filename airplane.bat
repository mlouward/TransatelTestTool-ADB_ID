@echo off
cd platform-tools/

REM Sets SELinux to Permisive (seems to be needed)
adb -s %1 shell su -c "setenforce 0" 
adb -s %1 shell "settings put global airplane_mode_on 1" & adb -s %1 shell su -c "am broadcast -a android.intent.action.AIRPLANE_MODE">nul && echo [!date!-!time:~0,8!] Airplane Mode On (%3) >> ../logs/airplanelogs.txt || echo [!date!-!time:~0,8!] %3 not plugged in. >> ../logs/airplanelogs.txt && goto End

timeout %2

adb -s %1 shell "settings put global airplane_mode_on 0" & adb -s %1 shell su -c "am broadcast -a android.intent.action.AIRPLANE_MODE">nul && echo [!date!-!time:~0,8!] Airplane Mode Off (%3) >> ../logs/airplanelogs.txt

:End
cd..