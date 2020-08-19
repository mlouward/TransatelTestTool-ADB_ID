@echo off
cd platform-tools\
setlocal enabledelayedexpansion

for /l %%F in (1, 1, %5) do (
	REM Delay between two consecutive calls ^(in seconds^)
	timeout 1 >nul
	adb -s %1 shell am start -a android.intent.action.CALL -d tel:%2 >nul && echo [!date!-!time:~0,8!] Call started. ^(FROM: %3, TO: %2, NB: %%F, DURATION: %4sec.^) >>..\logs\MOClog.txt || goto Error
	REM Delay before ending the call ^(in seconds^)
	timeout %4
	
	REM We get call state just before the end of the call
	REM 0: IDLE, 1: Active, 3: Dialing, 4: Alerting, 5: Incoming, 7: Disconnected
	REM See here for precisions: https://android.googlesource.com/platform/frameworks/base.git/+/master/telephony/java/android/telephony/PreciseCallState.java

	adb -s %1 shell "dumpsys telephony.registry | grep -i foregroundcallstate" >temp.txt
	REM End the call
	adb -s %1 shell input keyevent KEYCODE_ENDCALL && echo [!date!-!time:~0,8!] Call ended. ^(FROM: %3, TO: %2, NB: %%F, DURATION: %4sec.^) >>..\logs\MOClog.txt || goto Error
	REM Logs
	for /f "tokens=2 usebackq delims==" %%h in (`findstr /n "=" temp.txt ^| findstr "1:"`) do (
		echo %%h
		if "%%h"=="1" (set "state=Active")
		if "%%h"=="2" (set "state=OnHold")
		if "%%h"=="3" (set "state=Dialing")
		if "%%h"=="4" (set "state=Alerting")
		if "%%h"=="7" (set "state=Disconnected")
		if "%%h"=="8" (set "state=Disconnecting")
	)
	del temp.txt
	REM Log : Date/LoopNb/From/To/State
	echo [!date!-!time:~0,8!] MOC process successful. Final call state: "!state!" ^(FROM: %3, TO: %2, NB: %%F, DURATION: %4sec.^)>>..\logs\MOClog.txt
	echo [%time:~0,8%] MOC process successful.
)
goto End

:Error
echo Call was unsuccessful, make sure phone A is correctly plugged in.
echo [!date!-!time:~0,8!] Call unsuccesful. ^(FROM: %3, TO: %2, NB: %nb%, DURATION: %4sec.^)>>..\logs\MOClog.txt

:End
endlocal
echo. >> ..\logs\MOClog.txt
cd..