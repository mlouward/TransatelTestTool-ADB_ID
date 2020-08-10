@echo off
cd platform-tools/
setlocal enabledelayedexpansion

adb -s %1 shell su -c "content query --uri content://telephony/carriers --projection name:numeric:apn --where "_id=%2"">apninfos.txt || goto Failure
for /f "tokens=4,6,8 delims==: " %%F in (apninfos.txt) do (
	set "infos=NAME: %%F MCC/MNC: %%G APN: %%H"
)

adb -s %1 shell su -c "content update --uri content://telephony/carriers/preferapn --bind apn_id:i:%2" && goto Success || goto Failure 

:Success
echo [!date!-!time:~0,8!] Set default APN. (PHONE: %3, APN ID: %2, !infos!)>>..\logs\APNlog.txt 
goto End

:Failure
echo [!date!-!time:~0,8!] Could not set default APN. (PHONE: %3, APN ID: %2, !infos!)>>..\logs\APNlog.txt

:End
del apninfos.txt
endlocal
cd..