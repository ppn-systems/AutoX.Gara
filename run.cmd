@echo off
REM Run gara-server.exe from the same directory as this script

REM Optional: Set working directory to the script's location
cd /d "%~dp0"

REM Start server (without opening a new window)
"" "gara-server.exe"

REM Optional: Pause to keep CMD window open
REM pause