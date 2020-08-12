@echo off
cd platform-tools/

REM First SIM slot can use any sub ID excepted the one for sim slot 2.
REM We need both sub IDs to send commands to botsh SIM slots of a phone.
REM This file is used to find the sub ID for slot 2, by trying to send an sms 
REM using multiple sub IDs. Look for an sms coming from the MSISDN of the sim in slot 2.
REM The text of this SMS will be the sub ID of the slot 2 of the phone.

REM Usage in cmd: "findSim.bat <PhoneNumber>"
REM where <PhoneNumber> is the phone number to which send the messages.

for /L %%F IN (0,1,9) DO (
	adb shell "service call isms 7 i32 %%F s16 "com.android.mms.service" s16 "%1" s16 "null" s16 "%%F" s16 "null" s16 "null""
	timeout 1 > nul
)

echo OVER
pause