@echo off
cd platform-tools/

adb -s %1 shell su -c "content update --uri content://telephony/carriers/preferapn --bind apn_id:i:%2" && echo [%date%-%time:~0,8%] Set default APN. (PHONE: %3, APN ID: %2)>>..\logs\APNlog.txt || echo [%date%-%time:~0,8%] Could not set default APN. (PHONE: %3, APN ID: %2)>>..\logs\APNlog.txt

endlocal
cd..
