@echo off
IF "%~1" == "" GOTO Info

for %%f in (%1\*.epl*) do "%~d0%~p0EplAltEditor.exe" "%%f"
exit /b

:Info
echo Drag and drop a folder to unpack all epl files in it
echo:
set /p input="Press Enter to Quit"