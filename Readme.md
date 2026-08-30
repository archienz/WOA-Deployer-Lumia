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

1. Install Visual Studio 2022 or Build Tools with the .NET desktop workload and the .NET Framework 4.8 targeting pack.
2. Open `Source\WoaDeployer for Lumia.sln`.
3. Restore NuGet packages.
4. Build the **Release** configuration.

The output is `Source\Deployer.Lumia.Gui\bin\Release\WoaDeployer.exe`.

The Release zip on GitHub contains that graphical program and its libraries. The command-line project is in the source archive.

## 8. License

MIT License. See `LICENSE`.

Upstream project: [WOA-Project/WOA-Deployer-Lumia](https://github.com/WOA-Project/WOA-Deployer-Lumia)

## 9. Documents

- `CHANGELOG.md` — full change list in Simplified Technical English (ASD-STE100)
- `Docs/` — upstream guides

A condensed change list is in [section 10](#10-changelog-condensed) of this page.

## 10. Changelog (condensed)

Language: ASD-STE100 (Simplified Technical English).

This section is a short list of differences. For the full text, see `CHANGELOG.md`.

### 10.1 Identification

| Item | Value |
|---|---|
| Product name | WOA Deployer for Lumia |
| New version | 2.8.0 |
| Previous version | 2.7.1 |
| Date | 2026-08-30 |
| Upstream | WOA-Project / SuperJMN |

Version 2.8.0 is a modified 2.7.1. The primary task did not change: deploy Windows on ARM to a Lumia 950 or a Lumia 950 XL.

### 10.2 Section A — Bug fixes

| ID | Defect in 2.7.1 | Change in 2.8.0 |
|---|---|---|
| A1 | Disk stream increased the position by the requested count. A short read caused a wrong offset. | The stream increases the position by the number of bytes that it reads. |
| A2 | The software opened the phone disk with no share. The open can fail when Windows mounts the disk. | The software opens the disk with share for read and write. |
| A3 | Dispose did not override the stream dispose method. The volume lock can stay active. | The software uses the standard dispose path and unlocks the volume. |
| A4 | If the GPT signature was not present, the scan did not stop. | The scan reads a maximum of 64 sectors. If the signature is not found, the software stops with an error. |
| A5 | The SBL1 buffer used the size of the PLAT partition. The write can go past the end of the buffer. | The buffer uses the size of the SBL1 partition. If PLAT, DPP, or SBL1 is missing, the software stops. |
| A6 | A missing UniqueId caused a crash. | A missing UniqueId is an empty string. The software continues with the partition-name test. |
| A7 | An unknown RM code caused a dictionary crash. | The software shows an error that names the unknown type. |
| A8 | Enable Dual Boot used a merge of two boolean streams. The command can become available when dual boot is not possible. | The command is available only when the phone can do dual boot and the software is not busy. |
| A9 | A bad image index caused a crash during the boot-manager patch. | If the index is not valid, the software skips the patch and writes a warning. |
| A10 | The space allocator can set a Data size that is zero or less than zero. | If the calculated size is not positive, the software does not resize the partition. |
| A11 | The software can try to open a script that does not exist. | The software checks that the file or folder exists. |
| A12 | A `product.dat` line without a separator caused a crash. | The software ignores a bad line. If TYPE or NAME is missing, the software stops. |

### 10.3 Section B — Safety changes

| ID | Change |
|---|---|
| B1 | Wipe of Windows 10 Mobile: Yes/No dialog. Default answer is No. |
| B2 | Dual-boot deploy: Yes/No dialog. The dialog tells the operator to charge the phone and remove the microSD card. |
| B3 | Force Dual Boot and Force Single Boot: warning dialog. Default answer is No. |
| B4 | If more than one disk looks like a Lumia, the software stops. |
| B5 | Disk identification uses Qualcomm/Microsoft phone MMC identifiers. The 28–34 GB size limit is not used for that test. The fallback test uses 16–128 GB and the EFIESP, TZAPPS, and DPP names. |

### 10.4 Section C — Operational changes

| ID | Change |
|---|---|
| C1 | Local deployment scripts in `Source\Deployer.Lumia\Core\Deployment-Scripts`. If those files are present, the software does not download scripts from GitHub. |
| C2 | Default value of “Clean Downloaded folder before deployment” is False. |
| C3 | The `bootaa64.efi` patch applies only to Windows build 17763. |
| C4 | On Windows 10 version 1809 or later, the software uses the host DISM program. The bundled DISM program is the fallback. |
| C5 | After a download, the software calculates SHA-256. If a `*.sha256` file is next to the download, a hash mismatch stops the operation. |
| C6 | HTTP user agent is `WOADeployer-Lumia/2.8.0`. |
| C7 | If the operator starts the GUI with arguments, the software starts `Deployer.Lumia.Console.exe` when that file is present. |
| C8 | User interface text includes `.wim` and `.esd`. |
| C9 | Graphical programs target .NET Framework 4.8. Version 2.7.1 used 4.7.2 and 4.6.2. |

### 10.5 Section D — Items that did not change

- This software does not unlock the bootloader. Use WPInternals.
- A WIM/ESD deploy still needs Mass Storage mode.
- Camera, Windows Hello, and VoLTE on WOA are still not supported.
- The MIT license of the upstream project is still in effect.

### 10.6 Files that the operator uses

| File | Purpose |
|---|---|
| `WoaDeployer.exe` | Graphical deploy program (in the win-x86 zip) |
| `tools\flash-lumia-ffu.cmd` | Optional FFU flash with thor2 |

The command-line project is in the source archive. The binary package contains the graphical program.