@echo off
CHOICE /C YN /M "You are about to clear all files in this directory. Are you sure you want to continue?"
IF %errorlevel% EQU 1 goto Delete
IF %errorlevel% EQU 2 goto Abort

:Delete
set /a n=0
for %%F in (*) do (
	if "%%~xF" NEQ ".bat" (
		break>%%F
		set /a n+=1
	)
)
echo Cleared %n% files.
goto End

:Abort
echo No files have been deleted.

:End
pause
