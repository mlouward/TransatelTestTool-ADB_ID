@echo off
cd platform-tools/

REM First SIM slot can use any  sim ID excepted the one for sim slot 2.
REM This file is used to find the sim ID for slot 2, by trying to send an sms 
REM using multiple sim ID. Look for an sms coming from the MSISDN of the sim in slot 2.
REM The text of this SMS will be the sim ID of the slot 2 of the phone.

REM Usage in cmd: findSim.bat <PhoneNumber>

for /L %%F IN (0,1,9) DO (
	adb shell "service call isms 7 i32 %%F s16 "com.android.mms.service" s16 "%1" s16 "null" s16 "%%F" s16 "null" s16 "null""
	timeout 1 > nul
)

echo OVER
pause