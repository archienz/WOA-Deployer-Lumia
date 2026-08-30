# Changelog

Language: ASD-STE100 (Simplified Technical English).

This document describes WOA Deployer for Lumia **2.8.0**. The previous public version is **2.7.1**.

A condensed list of the same sections is at the bottom of `Readme.md` (GitHub repository home page).

## 1. Identification

| Item | Value |
|---|---|
| Product name | WOA Deployer for Lumia |
| New version | 2.8.0 |
| Base version | 2.7.1 |
| Date | 2026-08-30 |
| Upstream | WOA-Project / SuperJMN |

## 2. Summary of differences

Version 2.8.0 is a modified 2.7.1. The new version:

- Does not download deployment scripts when local scripts are present
- Asks the operator to confirm a wipe of Windows 10 Mobile
- Warns the operator before a forced dual-boot change
- Identifies the phone disk with more accuracy
- Stops if more than one disk looks like a phone
- Accepts `.esd` images in the user interface text
- Uses the host DISM program on Windows 10 1809 or later
- Records a SHA-256 hash for each download
- Does not contain a path to a private user profile

The new version does not change the primary task of the software: deploy Windows on ARM to a Lumia 950 or Lumia 950 XL.

## 3. Section A — Bug fixes

### A1. Disk stream read

**Before:** The disk stream increased the position by the requested count. A short read caused a wrong position.

**After:** The disk stream increases the position by the number of bytes that it reads.

**Effect:** The software reads the GPT and the SBL1 data with the correct offset.

### A2. Disk stream lock

**Before:** The software opened the phone disk with no share. The operation can fail when Windows mounts the disk.

**After:** The software opens the disk with share for read and write.

**Effect:** The software can read phone metadata in Mass Storage mode.

### A3. Disk stream dispose

**Before:** The dispose method did not override the stream dispose method. The volume lock can stay active.

**After:** The software uses the standard dispose path and unlocks the volume.

**Effect:** The PC can eject the phone disk after the operation.

### A4. GPT scan loop

**Before:** If the GPT signature was not present, the scan did not stop.

**After:** The scan reads a maximum of 64 sectors. If the GPT signature is not found, the software stops with an error.

**Effect:** The software does not hang on a bad disk.

### A5. SBL1 buffer size

**Before:** The software allocated the SBL1 buffer with the size of the PLAT partition.

**After:** The software allocates the buffer with the size of the SBL1 partition. If PLAT, DPP, or SBL1 is missing, the software stops with an error.

**Effect:** The software does not write past the end of the buffer.

### A6. Null disk identifier

**Before:** A missing UniqueId caused a crash.

**After:** The software treats a missing UniqueId as an empty string.

**Effect:** The software continues and uses the partition-name test.

### A7. Unknown phone type

**Before:** An unknown RM code caused a dictionary crash.

**After:** The software shows an error that names the unknown type.

**Effect:** The operator can see that the phone is not supported.

### A8. Dual-boot command enable

**Before:** The Enable Dual Boot command used a merge of two boolean streams. The command can become available when the phone cannot do dual boot.

**After:** The command is available only when the phone can do dual boot and the software is not busy.

**Effect:** The operator cannot start an unsafe dual-boot change from the Dual Boot page.

### A9. Image index range

**Before:** A bad image index caused a crash during the boot-manager patch.

**After:** If the index is not valid, the software skips the patch and writes a warning to the log.

**Effect:** Deployment can continue for a valid image.

### A10. Data partition resize

**Before:** The space allocator can try to set a Data size that is zero or less than zero.

**After:** If the calculated size is not positive, the software does not resize the partition.

**Effect:** The Data partition is not destroyed by a bad size.

### A11. Missing script path

**Before:** The software can try to open a script that does not exist.

**After:** The software checks that the file or folder exists.

**Effect:** The operator sees an error instead of a process start failure.

### A12. product.dat parse

**Before:** A line without a separator caused a crash.

**After:** The software ignores a bad line. If TYPE or NAME is missing, the software stops with an error.

**Effect:** The software does not crash on a malformed product.dat.

## 4. Section B — Safety changes

### B1. Wipe confirmation

The software shows a Yes/No dialog before a wipe of Windows 10 Mobile. The default answer is No.

### B2. Deploy confirmation

The software shows a Yes/No dialog before a dual-boot deploy. The dialog tells the operator to charge the phone and remove the microSD card.

### B3. Force Dual Boot warning

The Force Dual Boot and Force Single Boot commands show a warning. The default answer is No.

### B4. Multiple disk match

If more than one disk looks like a Lumia, the software stops. The operator must remove the other USB disks.

### B5. Disk identification

If the disk UniqueId contains `VEN_QUALCOMM&PROD_MMC_STORAGE` or `VEN_MSFT&PROD_PHONE_MMC_STOR`, the software accepts the disk. The software does not use the 28–34 GB size limit for that test.

If the UniqueId is not a match, the software uses a size range of 16–128 GB and the partition names EFIESP, TZAPPS, and DPP.

## 5. Section C — Operational changes

### C1. Local deployment scripts

The software includes the Lumia deployment scripts in `Source\Deployer.Lumia\Core\Deployment-Scripts`.

If those files are present, the software does not download scripts from GitHub.

### C2. Download folder

The default value of “Clean Downloaded folder before deployment” is False. The software does not delete cached files at each start.

### C3. Boot-manager patch

The software applies the `bootaa64.efi` patch only for Windows build 17763. For other builds, the software writes a log message and does not copy the old boot manager.

### C4. Host DISM

On Windows 10 version 1809 (build 17763) or later, the software uses the DISM program from the operating system. The bundled DISM program is the fallback.

### C5. Download hash

After a file download, the software calculates SHA-256 and writes it to the log. If a file named `*.sha256` is next to the download, the software compares the hash. If the hash is not the same, the software stops.

### C6. HTTP user agent

The HTTP client sends the user agent `WOADeployer-Lumia/2.8.0`.

### C7. Command-line launch

If the operator starts the GUI with arguments, the software starts `Deployer.Lumia.Console.exe` when that file is present.

### C8. Image types

The user interface text includes `.wim` and `.esd`.

### C9. Target framework

The graphical programs use .NET Framework 4.8. The previous version used 4.7.2 and 4.6.2.

## 6. Section D — Items that did not change

- The software still uses WPInternals to unlock the bootloader. This software does not unlock the bootloader.
- The software still needs Mass Storage mode for a WIM/ESD deploy.
- The software still does not support camera, Windows Hello, or VoLTE on WOA.
- The MIT license of the upstream project is still in effect.

## 7. Section E — Files that the operator uses

| File | Purpose |
|---|---|
| `WoaDeployer.exe` | Graphical deploy program |
| `tools\flash-lumia-ffu.cmd` | Optional FFU flash with thor2 |

The command-line project is in the source archive. The binary package contains the graphical program.

## 8. Section F — How to report a defect

1. Collect the log from the `Logs` folder next to `WoaDeployer.exe`.
2. Write the phone model (Talkman or Cityman) and the RM code.
3. Write the Windows image build number.
4. Open an issue on the GitHub repository of this fork.
