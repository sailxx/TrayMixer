# Security Policy

## Reporting a vulnerability

Please **do not** open a public issue. Report privately via
[GitHub Security Advisories](https://github.com/sailxx/TrayMixer/security/advisories/new).
You'll get a reply within a few days.

## What TrayMixer does and doesn't do

- Runs as the current user only (`asInvoker` manifest) — never asks for admin rights.
- Sends no telemetry and has no automatic update requests.
- Loads system DLLs from `System32` only (`SetDefaultDllDirectories` + `DefaultDllImportSearchPaths`), so a DLL dropped next to the exe is ignored.
- Writes only to `HKCU\Software\TrayMixer` (settings, hidden rows) and `HKCU\...\CurrentVersion\Run` (autostart, removable from the menu).
- Changes audio settings only through the official Windows Core Audio API, plus the `IPolicyConfig` interface Windows itself uses to switch the default device.

## Verifying releases

The Windows release workflow builds the Windows executable and installer from the tagged source and publishes a
SHA-256 checksums and signed build provenance for both files. Verify provenance with this command.

```bash
gh attestation verify TrayMixer.exe --repo sailxx/TrayMixer
```

If the release also includes an Authenticode signature, verify it separately on the downloaded executable.
