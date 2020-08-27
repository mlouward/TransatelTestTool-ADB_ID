@echo off
cd platform-tools\
setlocal enabledelayedexpansion

REM Turn on screen and unlock if necessary. Disables WiFi to make sure we use mobile data.
adb -s %1 shell input keyevent KEYCODE_WAKEUP & adb -s %1 shell input swipe 100 1000 100 0 & adb -s %1 shell input keyevent KEYCODE_MENU & adb -s %1 shell svc data enable & adb -s %1 shell svc wifi disable

REM Try to delete file at the start in case it is still there.
adb -s %1 shell "rm /storage/self/primary/Download/%3MB.zip" >nul

REM Download a file of given size
adb -s %1 shell "am start -a android.intent.action.VIEW -d "http://ipv4.download.thinkbroadband.com/%3MB.zip"" && echo [!date!-!time:~0,8!] Speedtest initiated. (PHONE: %2, SIZE: %3MB) >>..\logs\speedtestlog.txt || goto ErrorDownload

:Loop
endlocal
setlocal enabledelayedexpansion
REM Get state of download (not 0 when download starts)
adb -s %1 shell "dumpsys activity services | grep -i downloads" >state.txt
for %%a in (state.txt) do (
	if %%~za GTR 0 (
		echo !date!-!time:~0,11! > elapsed.txt
		echo Downloading... && echo [!date!-!time:~0,8!] Download started. ^(PHONE: %2, SIZE: %3MB^)>>..\logs\speedtestlog.txt
		goto Loop2
	)
)
goto Loop

:Loop2
endlocal
setlocal enabledelayedexpansion
REM Get state of download (0 if finished)
adb -s %1 shell "dumpsys activity services | grep -i downloads" >state.txt
for %%a in (state.txt) do (
	if not %%~za GTR 0 (echo !date!-!time:~0,11! >>elapsed.txt & goto ExLoop)
)
goto Loop2

:ExLoop
echo [!date!-!time:~0,8!] File download finished. (PHONE: %2, SIZE: %3MB) >>..\logs\speedtestlog.txt
del state.txt
REM Delete the file
adb -s %1 shell "rm /storage/self/primary/Download/%3MB.zip" >nul && echo [!date!-!time:~0,8!] File deleted from phone storage. (PHONE: %2, SIZE: %3MB) >>..\logs\speedtestlog.txt  || goto ErrorDelete

REM Can not clear cache because it clears all user data (needs human intervention to allow access to files).
REM adb -s %1 shell "pm clear com.android.chrome" >nul && echo [!date!-!time:~0,8!] Cache deleted for Chrome. (PHONE: %2, SIZE: %3MB) >>..\logs\speedtestlog.txt  || goto ErrorDelete

REM Turn off screen
adb -s %1 shell input keyevent KEYCODE_WAKEUP & adb -s %1 shell input keyevent KEYCODE_POWER
echo [%time:~0,8%] Speedtest process succesful. && echo [!date!-!time:~0,8!] Speedtest successful. (PHONE: %2, SIZE: %3MB) >>..\logs\speedtestlog.txt
goto End

:ErrorDownload
echo Unable to start the download. Make sure the phone is connected and has no password (swipe to unlock).
echo [!date!-!time:~0,8!] Could not start the download. (PHONE: %2, SIZE: %3MB) >>..\logs\speedtestlog.txt
goto End

:ErrorDelete
echo Unable to delete the downloaded file/clear cache. Please manually delete the folder `%3MB.zip` in your phone's download folder, and clear Chrome's cache.
echo [!date!-!time:~0,8!] Could not delete the downloaded file from phone storage. (PHONE: %2, SIZE: %3MB) >>..\logs\speedtestlog.txt

:End
endlocal
cd..
