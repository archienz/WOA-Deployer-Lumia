# Worked install log — Lumia 950 Talkman, Windows 11 22H2

Date: 2026-08-30  
Result: **Deployment successful** (WOA Deployer log line at 20:38:43 +10:00)

This is a record of files and steps that completed on a retail Lumia 950 (Talkman). It is not a guarantee for every phone.

## 1. Phone

| Item | Value |
|---|---|
| Marketing name | Lumia 950 |
| Codename | Talkman |
| Product | RM-1104 |
| Product code | 059X4V9 |
| Variant | Single SIM |
| Hardware | 2112 |
| Stock firmware (before unlock) | 01078.00017.15452.59001 |
| Target OS | Windows 11 Pro ARM64, build **22621.1** (22H2) |

Do not use this file list on a Lumia 950 XL (Cityman / RM-1085) or an AT&T Lumia 950 (RM-1105).

## 2. Files that were used

### 2.1 WPInternals (bootloader unlock)

Tool: **WPInternals 2.9.2** (x64).

Microsoft firmware download in WPInternals does nothing (the servers are gone). These local files were placed where WPInternals expects them (`C:\ProgramData\WPInternals` / the in-app repository):

| File | Role | Result |
|---|---|---|
| `RM1104_1078.0017.10586.13053.15482.02FAFD_retail_prod_signed.ffu` | Stock Windows 10 Mobile FFU for **this** Talkman | Used for unlock / profile |
| `RM1085_10586_donor.ffu` | Donor FFU (Cityman, **10.0.10586.318** boot files) | Used so WPInternals can patch `mobilestartup.efi` |
| `MPRG8992_fh.ede` | Emergency programmer (8992) | Present for emergency flash |
| `RM1104_fh.edp` | Emergency payload for RM-1104 | Present for emergency flash |

The donor is **not** the same version as a WOA FFU. It is a Windows 10 Mobile boot-file donor that WPInternals can patch.

Unlock: **succeeded**. After unlock, Mass Storage mode showed disk **MSFT Phone MMC Stor**.

### 2.2 Windows image (WOA Deployer)

| File | Role | Result |
|---|---|---|
| `install.esd` (614 MB, UUP `professional_en-us.esd`, SHA-1 `B8091809EF02A7F412DCFB15BE0DDF2B275EBAB3`) | Incomplete UUP metadata ESD | **Failed.** DISM apply index 3 stopped at 96% with code **1812** (missing resource). Do not use. |
| `install.wim` (3.46 GB) | Converted UUP set **22621.1** ARM64 Pro, LZX, index **1** Windows 11 Pro | **Worked.** DISM `/Apply-Image /Index:1` exit 0. |

How `install.wim` was built:

- UUP dump id `8c9cea2a-b4d5-49dd-82c4-f79bfac1108e` (Windows 11 22H2 22621.1 arm64)
- Converter: uup-converter-wimlib, `SkipISO=1`, `SkipApps=1`
- Output: `22621.1.220506-1250.NI_RELEASE_CLIENTPRO_OEMRET_A64FRE_EN-US\sources\install.wim`

WOA Deployer **2.8.0** rejected that WIM (`Invalid WIM file` — UTF-16 XML). **2.8.1** reads it.

### 2.3 WOA Deployer

| Item | Value |
|---|---|
| Version that completed the install | **2.8.1** (`WoaDeployer.exe` file version 2.8.1.0) |
| Mode | Mass Storage, disk **Online** |
| Phone detected | Talkman, SingleSim |
| Image | `install.wim` index 1 |
| Compact OS | Off |
| Host DISM | `C:\Windows\SysNative\dism.exe` 10.0.26100.x |
| Apply target | Windows volume `I:\` (NTFS) |
| EFI target | `F:\` then SYSTEM (FAT32) |
| Local scripts | Used `Core\Deployment-Scripts` (no GitHub script download) |

Log line: `Deployment successful`

### 2.4 UEFI / boot (downloaded by Deployer)

| File | Source | SHA-256 | Result |
|---|---|---|---|
| `MSM8992.UEFI.Lumia.950.zip` | WOA-Project/Lumia950XLPkg **2.24b** | `80104BD273D34365ADD263D80198386B0D406F3A4FF32DF9A351073E118B84CC` | Copied `UEFI.elf` |
| `UEFI Loader for 8992 and 8994` | LumiaWOA Azure Boot Shim build 174 | `9FAC2588C1BA9C58EE5B85DF041F3A9FBA20CF048ABE673D6C6C10B6E8496483` | Copied `BootShim.efi` |

`bcdboot I:\Windows /f UEFI` exit 0.  
`bcdedit` testsigning on, nointegritychecks on: exit 0.  
Legacy `bootaa64.efi` patch: **skipped** (only for build 17763).

### 2.5 Drivers (Lumia-Drivers v2502.03)

All `/Add-Driver /Recurse` calls completed with DISM exit 0. Image version reported: **10.0.22621.1**.

| Package | Result |
|---|---|
| `HARDWARE.INPUT.SYNAPTICS_RMI4_F12_WIN10.zip` | 6 INF installed |
| `DEVICE.SOC_QC899X.TALKMAN.zip` | Installed |
| `DEVICE.SOC_QC899X.TALKMAN_DESKTOP.zip` | Installed |
| `HARDWARE.USB.MMO_USBC.zip` | Installed |
| `OEM.SOC_QC899X.MMO.zip` | Installed |
| `OEM.SOC_QC899X.MMO_MINIMAL.zip` | Installed |
| `GRAPHICS.SOC_QC899X.MMO_DESKTOP.zip` | Installed |
| `HARDWARE.CAMERA.MMO_8992.zip` | INF installed (camera still has no working WOA driver) |
| `PLATFORM.SOC_QC899X.8992.zip` | Installed |
| `PLATFORM.SOC_QC899X.8992_MINIMAL.zip` | Installed |
| `PLATFORM.SOC_QC899X.BASE.zip` | Installed |
| `PLATFORM.SOC_QC899X.BASE_MINIMAL.zip` | Installed |
| `PLATFORM.SOC_QC899X.LATE_SOC.zip` | Installed |
| `PLATFORM.SOC_QC899X.MMO.zip` | Installed |
| `PLATFORM.SOC_QC899X.MMO_DESKTOP.zip` | Installed |
| `SUPPORT.DESKTOP.BASE.zip` | Installed |
| `SUPPORT.DESKTOP.EXTRAS.zip` | Installed |
| `SUPPORT.DESKTOP.MMO_EXTRAS.zip` | Installed |
| `SUPPORT.DESKTOP.MOBILE_BRIDGE.zip` | 8 INF installed |
| `SUPPORT.DESKTOP.MOBILE_COMPONENTS.zip` | 3 INF installed |
| `SUPPORT.DESKTOP.MOBILE_RIL.zip` | 3 INF installed |
| `SUPPORT.DESKTOP.MOBILE_RIL_EXTRAS.zip` | 1 INF installed |
| `Changelog.zip` | Shown in UI only |

## 3. Sequence that worked

1. Charge the phone. Remove microSD. Use a USB-A cable (USB-C PD can make the phone try to charge the PC).
2. Put the four WPInternals files above into the WPInternals repository.
3. Unlock the bootloader with WPInternals. Wait until unlock finishes.
4. WPInternals: **Switch to Mass Storage**. In Disk Management, set the phone disk **Online**. Disconnect other USB disks.
5. Run `WoaDeployer.exe` **2.8.1** as administrator.
6. Select `install.wim` (the 3.46 GB converted file). Select **Windows 11 Pro**.
7. Confirm the dialog. Wait until **Deployment successful**.
8. Eject the phone disk. Reboot the phone. Leave the cable connected for the first setup.

## 4. What did not work (do not repeat)

| Attempt | Error | Cause |
|---|---|---|
| `install.esd` index 3 (Windows 11 Pro) | DISM **1812** at ~96% | Incomplete UUP metadata ESD; missing blobs (first missing: `Windows.WARP.JITService.exe`) |
| Same WIM in Deployer **2.8.0** | Invalid WIM file | UTF-16 WIM XML not read. Fixed in **2.8.1**. |
| WPInternals “Download All” | No files | Microsoft firmware servers are dead |

## 5. Not in this deploy

- Dual boot with Windows 10 Mobile was not the completed path in the successful 20:07–20:38 run (Windows partition was created and the 22621 image applied to it).
- Camera, Windows Hello, VoLTE remain unsupported on LumiaWOA.
- Store apps were not slipstreamed into this WIM (`SkipApps=1`).

## 6. Log file

WOA Deployer wrote `Logs\Log-YYYYMMDD.txt` next to `WoaDeployer.exe`. The successful run ends with:

```
Deployment successful
```

## 7. First boot after that log line

Deployer success is not the same as a finished Windows first boot.

| What you see | What it was | What to do |
|---|---|---|
| "The computer restarted unexpectedly…" loop | Setup still marked in progress; Surface `SilentProvisioner`; Win11 TPM/RAM checks | Run `tools\fix-win11-setup-loop.cmd` while the phone is in Mass Storage. See `Docs/Fix-First-Boot-Loop.md`. |
| Login asks for a password; two Administrator tiles | No OOBE user was created; built-in Administrator is disabled | Sign-in options → the account that is not the disabled Administrator. Wait if it says Preparing Windows. |
| Ease of Access does nothing | Do not replace `utilman.exe` with an x64 program | The phone is ARM64. Use the script on the PC. |
| Bluescreen then reboot | FM radio services crashed; no minidump yet | The script disables those services and turns auto-reboot off. |

On this phone, after the hive fix, **Preparing Windows** is progress. Leave it on charge and wait.
