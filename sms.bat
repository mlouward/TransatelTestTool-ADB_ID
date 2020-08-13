@echo off
cd platform-tools\
setlocal enabledelayedexpansion

adb -s %1 shell "service call isms 7 i32 1 s16 "com.android.mms.service" s16 "%2" s16 "null" s16 "%5" s16 "null" s16 "null"" && echo [!date!-!time:~0,8!] SMS Sent. (FROM: %3, TO: %2, NB: %4, TEXT: %5)>>..\logs\SMSlog.txt || goto Error

goto End

:Error
echo Unable to send the SMS. Make sure that the correct phones are connected.
echo [!date!-!time:~0,8!] Unable to send SMS. (FROM: %3, TO: %2, NB: %4, TEXT: %5)>>..\logs\SMSlog.txt

:End
endlocal
cd..