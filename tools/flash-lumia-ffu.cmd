@echo off
setlocal EnableExtensions EnableDelayedExpansion
title WOA Deployer 2.8.1  -  FFU flash
color 0B

net session >nul 2>&1
if errorlevel 1 (
  echo Requesting Administrator...
  powershell -NoProfile -Command "Start-Process -LiteralPath '%~f0' -Verb RunAs -ArgumentList '%*'"
  exit /b 1
)

set "THOR2=%ProgramFiles(x86)%\Microsoft Care Suite\Windows Device Recovery Tool\thor2.exe"
set "FFU=%~1"

if "%FFU%"=="" (
  echo.
  echo Usage:
  echo   flash-lumia-ffu.cmd "D:\images\RM1104_....ffu"
  echo.
  echo Use an RM-1104 Talkman FFU for Lumia 950.
  echo Use an RM-1085 Cityman FFU for Lumia 950 XL.
  echo This procedure removes Windows 10 Mobile. Dual boot is not available.
  echo.
  pause
  exit /b 1
)

echo.
echo ============================================================
echo   WOA Deployer 2.8.1  -  FFU flash
echo ============================================================
echo.
echo This procedure erases the phone.
echo Dual boot with Windows 10 Mobile is not available.
echo.
echo Checklist
echo   1. Charge the phone to 100 percent
echo   2. Remove the microSD card
echo   3. Disconnect other USB disks
echo   4. WPInternals: Manual mode, Switch to Flash mode
echo.

if not exist "%THOR2%" (
  echo ERROR: thor2.exe not found
  echo   %THOR2%
  echo Install Windows Device Recovery Tool.
  echo.
  pause
  exit /b 1
)
if not exist "%FFU%" (
  echo ERROR: FFU file not found
  echo   %FFU%
  echo.
  pause
  exit /b 1
)

echo thor2:
echo   %THOR2%
echo FFU:
echo   %FFU%
echo.
echo SHA256:
certutil -hashfile "%FFU%" SHA256
echo.

set "CONFIRM="
set /p "CONFIRM=Type WIPE to flash, or press Enter to cancel: "
if /i not "!CONFIRM!"=="WIPE" (
  echo Cancelled. No data was written.
  pause
  exit /b 0
)

echo.
echo Flashing. Do not disconnect the phone.
echo.
"%THOR2%" -mode uefiflash -erase_data -reboot -ffufile "%FFU%"
set "ERR=!ERRORLEVEL!"
echo.
if not "!ERR!"=="0" (
  color 0C
  echo thor2 failed with exit code !ERR!
  pause
  exit /b !ERR!
)

color 0A
echo The flash command completed.
echo Keep the phone connected during the first Windows setup.
echo After setup, inject drivers with DriverUpdater from the LumiaWOA project.
echo Use 950.xml for Talkman. Use 950xl.xml for Cityman.
echo.
pause
exit /b 0
