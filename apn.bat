@echo off
cd platform-tools/

REM Add APN: (needs name, numeric and apn)
adb shell su -c "content insert --uri content://telephony/carriers --bind name:s:"%name%" --bind numeric:s:"%num1%%num2%" --bind type:s:"default,supl" --bind mcc:i:%num1% --bind mnc:i:%num2% --bind apn:s:"%apn%""

adb shell su -c "content query --uri content://telephony/carriers --where "name='name'"" > ../carriers.txt

adb shell su -c "content update --uri content://telephony/carriers/preferapn --bind apn_id:i:%idPref%"
cd..
pause


REM adb shell "su -c cat /data/user_de/0/com.android.providers.telephony/shared_prefs/preferred-apn.xml" > ../preferred-apn.xml