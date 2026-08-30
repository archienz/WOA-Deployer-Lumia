@echo off
setlocal EnableExtensions EnableDelayedExpansion
title WOA Deployer 2.8.1 - first-boot setup loop fix
color 0B

net session >nul 2>&1
if errorlevel 1 (
  echo This script needs Administrator.
  powershell -NoProfile -Command "Start-Process -LiteralPath '%~f0' -Verb RunAs"
  exit /b 1
)

echo.
echo ============================================================
echo   Fix Windows 11 first-boot loop on Lumia WOA
echo ============================================================
echo.
echo Use this when the phone shows:
echo   "The computer restarted unexpectedly or encountered an
echo    unexpected error. Windows installation cannot proceed."
echo.
echo Put the phone in UEFI Mass Storage (Developer Menu) or
echo WPInternals Mass Storage. In Disk Management, set the
echo phone disk Online.
echo.
echo This script does not replace utilman.exe.
echo.

set "WINVOL="
for %%L in (I J K L M N O P Q R S T U V W X Y D E F G H) do (
  if exist "%%L:\Windows\System32\config\SYSTEM" (
    if exist "%%L:\Windows\System32\ntdll.dll" (
      set "WINVOL=%%L:"
      goto :FOUND
    )
  )
)

echo ERROR: No Windows folder was found on a mass-storage volume.
echo Connect the phone, set the disk Online, then run again.
pause
exit /b 1

:FOUND
echo Windows volume: %WINVOL%
echo.

reg unload HKLM\WOA_SYS >nul 2>&1
reg unload HKLM\WOA_SW  >nul 2>&1

reg load HKLM\WOA_SYS "%WINVOL%\Windows\System32\config\SYSTEM"
if errorlevel 1 (
  echo ERROR: Could not load SYSTEM hive.
  pause
  exit /b 1
)
reg load HKLM\WOA_SW "%WINVOL%\Windows\System32\config\SOFTWARE"
if errorlevel 1 (
  echo ERROR: Could not load SOFTWARE hive.
  reg unload HKLM\WOA_SYS >nul 2>&1
  pause
  exit /b 1
)

echo Clearing setup-restart flags...
reg add HKLM\WOA_SYS\Setup /v SystemSetupInProgress /t REG_DWORD /d 0 /f >nul
reg add HKLM\WOA_SYS\Setup /v SetupType /t REG_DWORD /d 0 /f >nul
reg add HKLM\WOA_SYS\Setup /v SetupPhase /t REG_DWORD /d 0 /f >nul
reg add HKLM\WOA_SYS\Setup /v OOBEInProgress /t REG_DWORD /d 0 /f >nul
reg add HKLM\WOA_SYS\Setup /v RestartSetup /t REG_DWORD /d 0 /f >nul
reg delete HKLM\WOA_SYS\Setup /v CmdLine /f >nul 2>&1
reg add HKLM\WOA_SYS\Setup\Status\ChildCompletion /v setup.exe /t REG_DWORD /d 3 /f >nul

echo Adding Windows 11 hardware bypasses (no TPM, 3 GB RAM)...
reg add HKLM\WOA_SYS\Setup\LabConfig /f >nul
reg add HKLM\WOA_SYS\Setup\LabConfig /v BypassTPMCheck /t REG_DWORD /d 1 /f >nul
reg add HKLM\WOA_SYS\Setup\LabConfig /v BypassSecureBootCheck /t REG_DWORD /d 1 /f >nul
reg add HKLM\WOA_SYS\Setup\LabConfig /v BypassRAMCheck /t REG_DWORD /d 1 /f >nul
reg add HKLM\WOA_SYS\Setup\LabConfig /v BypassCPUCheck /t REG_DWORD /d 1 /f >nul
reg add HKLM\WOA_SYS\Setup\LabConfig /v BypassStorageCheck /t REG_DWORD /d 1 /f >nul
reg add HKLM\WOA_SYS\Setup\LabConfig /v BypassDiskCheck /t REG_DWORD /d 1 /f >nul
reg add HKLM\WOA_SYS\Setup\MoSetup /v AllowUpgradesWithUnsupportedTPMOrCPU /t REG_DWORD /d 1 /f >nul

reg add HKLM\WOA_SW\Microsoft\Windows\CurrentVersion\OOBE /v BypassNRO /t REG_DWORD /d 1 /f >nul
reg add HKLM\WOA_SW\Microsoft\Windows\CurrentVersion\Setup\State /v ImageState /t REG_SZ /d IMAGE_STATE_COMPLETE /f >nul

echo Disabling FM radio services that crash on first boot...
reg add "HKLM\WOA_SYS\ControlSet001\Services\MobileFrequencyModulationService" /v Start /t REG_DWORD /d 4 /f >nul 2>&1
reg add "HKLM\WOA_SYS\ControlSet001\Services\FM Radio Miniport Interface" /v Start /t REG_DWORD /d 4 /f >nul 2>&1

echo Crash dumps on, no auto reboot (so a BSOD stop code stays on screen)...
reg add HKLM\WOA_SYS\ControlSet001\Control\CrashControl /v AutoReboot /t REG_DWORD /d 0 /f >nul
reg add HKLM\WOA_SYS\ControlSet001\Control\CrashControl /v CrashDumpEnabled /t REG_DWORD /d 3 /f >nul
reg add "HKLM\WOA_SYS\ControlSet001\Control\Session Manager\Power" /v HiberbootEnabled /t REG_DWORD /d 0 /f >nul

echo Unloading hives...
reg unload HKLM\WOA_SYS
reg unload HKLM\WOA_SW

if exist "%WINVOL%\Windows\OEM\SilentProvisioner.exe" (
  echo Disabling Surface SilentProvisioner.exe
  ren "%WINVOL%\Windows\OEM\SilentProvisioner.exe" SilentProvisioner.exe.disabled 2>nul
)
if exist "%WINVOL%\Windows\OEM\SilentProvisionerL.exe" (
  echo Disabling Surface SilentProvisionerL.exe
  ren "%WINVOL%\Windows\OEM\SilentProvisionerL.exe" SilentProvisionerL.exe.disabled 2>nul
)
if exist "%WINVOL%\Windows\OEM\ApplicationProvisioner.exe" (
  echo Disabling Surface ApplicationProvisioner.exe
  ren "%WINVOL%\Windows\OEM\ApplicationProvisioner.exe" ApplicationProvisioner.exe.disabled 2>nul
)

echo.
echo Done. Next:
echo   1. Eject the phone disk in Explorer
echo   2. Unplug USB
echo   3. Power the phone off, then on
echo   4. Wait. "Preparing Windows" can take a long time on eMMC.
echo   5. At login, open Sign-in options and pick the account that
echo      is not the disabled Administrator, if more than one is listed.
echo.
pause
exit /b 0
