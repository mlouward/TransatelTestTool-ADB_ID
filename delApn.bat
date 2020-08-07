@echo off
cd platform-tools/

adb -s %1 shell su -c "content delete --uri content://telephony/carriers --where "_id=%2"" && echo [!date!-!time:~0,8!] APN deleted. (PHONE: %3, APN_ID: %2)>>..\logs\APNlog.txt || echo [!date!-!time:~0,8!] Could not delete APN. (PHONE: %3, APN_ID: %2)>>..\logs\APNlog.txt

cd ..