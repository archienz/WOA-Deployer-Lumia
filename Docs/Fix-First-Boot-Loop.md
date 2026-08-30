# First-boot loop, login, and bluescreen (Windows 11 on Lumia)

This matches a completed Talkman install of Windows 11 Pro ARM64 **22621.1** with WOA Deployer **2.8.1**.

## 1. "The computer restarted unexpectedly…"

After Deployer says **Deployment successful**, the phone may boot into:

> The computer restarted unexpectedly or encountered an unexpected error.  
> Windows installation cannot proceed. Click OK to restart…

Clicking OK only repeats the loop.

### Cause

Windows Setup is still marked as running. On this image the SYSTEM hive had:

| Value | Stuck value | Meaning |
|---|---|---|
| `Setup\CmdLine` | `oobe\windeploy.exe` | OOBE deployer |
| `Setup\SetupType` | 2 | OOBE / specialize |
| `Setup\SetupPhase` | 4 | In progress |
| `Setup\SystemSetupInProgress` | 1 | Setup not finished |
| `Setup\Status\ChildCompletion\setup.exe` | 1 | Setup child failed |

A converted UUP **OEMRET** WIM is often a Surface ARM64 image. It runs `\Windows\OEM\SilentProvisioner.exe` during specialize and first logon. That program is not valid on a Lumia and can crash Setup.

The Lumia 950 has **3 GB RAM** and **no TPM 2.0**. Windows 11 22621 also fails hardware checks unless LabConfig bypass values are set.

### Fix (from the PC)

1. On the phone: Developer Menu → **Mass Storage** (or WPInternals Mass Storage).
2. On the PC: Disk Management → phone disk **Online**.
3. Run as administrator:

```
tools\fix-win11-setup-loop.cmd
```

The script loads the Windows hive on the phone disk and:

- Sets `setup.exe` ChildCompletion to **3**
- Sets SetupType / SetupPhase / SystemSetupInProgress / OOBEInProgress to **0**
- Deletes `CmdLine`
- Adds LabConfig bypasses (TPM, Secure Boot, RAM, CPU, storage)
- Disables FM radio services that crash on first boot
- Renames Surface `SilentProvisioner*.exe` if present
- Turns crash dumps on and auto-reboot **off** (so a later BSOD stop code stays on screen)

4. Eject the phone disk. Unplug USB. Power the phone off, then on.

Do **not** replace `utilman.exe` with a program built for x64. The phone runs **ARM64**. An x64 helper does nothing when you tap Ease of Access.

## 2. Login asks for a password / two Administrator accounts

### Cause

The hive fix above marks Setup complete so the error box goes away. It does **not** create a user. The built-in **Administrator** account stays disabled. Windows 11 can still show an Administrator tile with no password that works.

### What to do on the login screen

1. Tap **Sign-in options** (not Ease of Access).
2. If two Administrator tiles appear, pick the one that is **not** the disabled account.
3. If it says **Preparing Windows**, leave the phone plugged in to USB power and wait. eMMC is slow. This can take many minutes.
4. Do not keep tapping OK on the old setup error. That only reboots.

A later boot may still say "incorrect password" on the disabled Administrator tile. Use Sign-in options and the other tile, or wait for Preparing Windows to finish.

## 3. Bluescreen after login

On the Talkman 22621 run, no minidump was written (crash dump was off and Windows rebooted immediately). User-mode crash reports that *were* present:

- `FMInterfaceSvc` (FM Radio Interface)
- `MobileFrequencyModulationService`

Those two services are set to disabled by `fix-win11-setup-loop.cmd`.

If it bluescreens again after the script:

- Leave auto-reboot off (the script already did that)
- Photograph the **stop code** on the screen
- Then use Mass Storage and copy `Windows\Minidump\*.dmp`

## 4. Ease of Access does nothing

Do not swap `utilman.exe` for a helper compiled with `Framework64\csc.exe`. That produces an **x64** file. ARM64 Windows will not run it, so the accessibility button does nothing.

Use `tools\fix-win11-setup-loop.cmd` from the PC instead.

## 5. After it works

Keep the phone on charge for the first hour. Drivers keep installing. Camera, Windows Hello, and VoLTE still do not work on LumiaWOA.
