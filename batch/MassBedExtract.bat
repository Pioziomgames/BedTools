@echo off
IF "%~1" == "" GOTO Info

for %%f in (%1\*.bed*) do "%~d0%~p0BedEditor.exe" "%%f"
exit /b

:Info
echo Drag and drop a folder to unpack all bed files in it
echo:
set /p input="Press Enter to Quit"