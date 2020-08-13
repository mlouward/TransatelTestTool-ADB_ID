@echo off
cd platform-tools\
setlocal enabledelayedexpansion

REM Set default Sim card to use for calls
adb -s %1 shell "settings put global multi_sim_voice_call %7"


REM Delay between two consecutive calls (in seconds)
timeout 2 >nul
adb -s %1 shell am start -a android.intent.action.CALL -d tel:%4 > nul && echo Calling "%4"... && echo. && echo [!date!-!time:~0,8!] Call started by %3. (FROM: %3, TO: %4, NB: %6, DURATION: %5sec.) >>..\logs\MTClog.txt || goto Error
:LoopMTC
	endlocal
	adb -s %2 shell "dumpsys telephony.registry | grep mCallState" >state.txt || goto ErrorB
	findstr /n "=" state.txt | findstr "1:" >temp.txt
	setlocal enabledelayedexpansion
	FOR /F "tokens=2 delims==" %%H IN (temp.txt) DO (
		set /a s=%%H
		if !s!==1 (
			REM Delay before answering
			timeout 1 >nul
			REM B number answers incoming call
			adb -s %2 shell input keyevent KEYCODE_CALL >nul && echo Call answered. && echo. && echo [!date!-!time:~0,8!] Call answered by %4. ^(FROM: %3, TO: %4, NB: %6, DURATION: %5sec.^) >>..\logs\MTClog.txt || goto ErrorB
			goto ExLoopMtc
		) else (goto LoopMTC)
	)
:ExLoopMtc
	del state.txt
	del temp.txt
	REM Call duration
	timeout %5
	REM Get call state on phone A
	adb -s %1 shell "dumpsys telephony.registry | grep -i foregroundcallstate" >temp.txt || goto ErrorA
	findstr /n "=" temp.txt | findstr "1:">temp2.txt
	REM If call is active (%%G == 1), we hang up from phone A.
	for /f "tokens=2 delims==" %%G in (temp2.txt) do (
		if "%%G"=="1" (
			adb -s %1 shell input keyevent KEYCODE_ENDCALL && echo Call ended. && echo [!date!-!time:~0,8!] Call terminated by %3. ^(FROM: %3, TO: %4, NB: %6, DURATION: %5sec.^) >>..\logs\MTClog.txt || goto ErrorA
		)
	)
	del temp.txt
	del temp2.txt
	echo [!date!-!time:~0,8!] MTC Process successful. (FROM: %3, TO: %4, NB: %6, DURATION: %5sec.) >>..\logs\MTClog.txt
	echo [%time:~0,8%] MTC process successful.
	goto End
	
:Error
echo Unable to make the call. Make sure both phones are connected.
echo [!date!-!time:~0,8!] Call not initiated. (FROM: %3, TO: %4, NB: %6, DURATION: %5sec.) >>..\logs\MTClog.txt
goto End

:ErrorA
echo Unable to hang up from phone A. Make sure both phones are connected.
echo [!date!-!time:~0,8!] Phone A could not hang up. (FROM: %3, TO: %4, NB: %6, DURATION: %5sec.) >>..\logs\MTClog.txt
goto End

:ErrorA2
echo Couldn't verify that call state is "Active".
echo [!date!-!time:~0,8!] Connection not established. (FROM: %3, TO: %4, NB: %6, DURATION: %5sec.) >>..\logs\MTClog.txt
goto End

:ErrorB
echo Unable to answer from phone B. Make sure both phones are connected.
echo [!date!-!time:~0,8!] Phone B could not answer. (FROM: %3, TO: %4, NB: %6, DURATION: %5sec.) >>..\logs\MTClog.txt

:End
endlocal
cd ..