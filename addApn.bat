@echo off
cd platform-tools/

adb -s %1 shell su -c "content insert --uri content://telephony/carriers --bind name:s:"%2" --bind numeric:s:"%3%4" --bind type:s:"default,supl" --bind mcc:i:%3 --bind mnc:i:%4 --bind apn:s:"%5"" && echo [!date!-!time:~0,8!] APN added. (PHONE: %6, NAME: %2, MCC/MNC: %3%4, APN: %5)>>..\logs\APNlog.txt || echo [!date!-!time:~0,8!] Could not add APN. (PHONE: %6, NAME: %2, MCC/MNC: %3%4, APN: %5)>>..\logs\APNlog.txt

cd ..