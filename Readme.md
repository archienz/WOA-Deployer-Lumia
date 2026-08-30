# WOA Deployer for Lumia 2.8.0

This software installs Windows on ARM on a Lumia 950 (Talkman) or a Lumia 950 XL (Cityman).

This release is a modified version of WOA Deployer for Lumia 2.7.1 by José Manuel Nieto (SuperJMN) and the WOA Project.

## 1. Scope

The software can:

- Deploy a Windows image (`.wim` or `.esd`) to a Lumia 950 or Lumia 950 XL
- Keep Windows 10 Mobile (dual boot) or remove it
- Enable or disable dual boot after deployment

The software does not unlock the bootloader. Unlock the bootloader with WPInternals before you use this software.

## 2. Requirements

- A Windows PC (Windows 10 version 1809 or later)
- A Lumia 950 (RM-1104 / RM-1118) or Lumia 950 XL (RM-1085 / RM-1116) with an unlocked bootloader
- A USB-C cable
- A Windows 10 or Windows 11 ARM64 image (`.wim` or `.esd`)

Do not use this procedure on an AT&T Lumia 950 (RM-1105) unless you know that your image supports that model.

## 3. Install the graphical application

1. Download `WOA-Deployer-Lumia-2.8.0-win-x86.zip` from the GitHub Releases page.
2. Extract the archive to a folder.
3. Right-click `WoaDeployer.exe`.
4. Select **Run as administrator**.

## 4. Deploy Windows (graphical procedure)

1. Charge the phone to 100 percent.
2. Remove the microSD card.
3. Disconnect other USB disks from the PC.
4. Put the phone in Mass Storage mode with WPInternals.
5. In Disk Management, make sure that the phone disk is **Online**.
6. Start `WoaDeployer.exe` as administrator.
7. Select the `.wim` or `.esd` file.
8. Select the Windows edition in the image list.
9. Select **Keep Windows 10 Mobile** or **Overwrite (Wipe) Windows 10 Mobile**.
10. Confirm the dialog.
11. Wait until the procedure ends.
12. Disconnect the phone with the operating system **Eject** function.

## 5. Optional: flash a ready-made FFU

This method removes Windows 10 Mobile. Dual boot is not available.

1. Put the phone in Flash mode with WPInternals.
2. Open an administrator Command Prompt.
3. Run:

```
tools\flash-lumia-ffu.cmd "C:\path\to\RM1104_....ffu"
```

4. Type `WIPE` to start the flash.
5. After the first Windows setup, inject drivers with DriverUpdater from the LumiaWOA project. Use `950.xml` for Talkman. Use `950xl.xml` for Cityman.

## 6. Safety rules

- Do not use **Force Dual Boot** during the first setup or during driver injection.
- Do not format partitions that Windows shows when Mass Storage starts.
- Do not flash a Cityman (RM-1085) image to a Talkman (RM-1104) phone.
- Make a backup of the phone before you start.

## 7. Build from source

1. Install Visual Studio 2022 or Build Tools with the .NET desktop workload and .NET Framework 4.7.2 targeting pack.
2. Open `Source\WoaDeployer for Lumia.sln`.
3. Restore NuGet packages.
4. Build the **Release** configuration.

The output is `Source\Deployer.Lumia.Gui\bin\Release\WoaDeployer.exe`.

The Release zip on GitHub contains that graphical program and its libraries. The command-line project is in the source archive.

## 8. License

MIT License. See `LICENSE`.

Upstream project: [WOA-Project/WOA-Deployer-Lumia](https://github.com/WOA-Project/WOA-Deployer-Lumia)

## 9. Documents

- `CHANGELOG.md` — changes in Simplified Technical English (ASD-STE100)
- `Docs/` — upstream guides
