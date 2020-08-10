@echo off
REM Download Android Debug Bridge if not already here
set "url=https://dl.google.com/android/repository/platform-tools-latest-windows.zip"
set "goal=platform-tools-latest-windows.zip"
echo Downloading ADB...
powershell -Command "(New-Object Net.WebClient).DownloadFile('%url%', '%goal%')"
echo Download finished
powershell Expand-Archive -Force -LiteralPath '%goal%' -DestinationPath '%cd%'
del %goal%
echo File unzipped!