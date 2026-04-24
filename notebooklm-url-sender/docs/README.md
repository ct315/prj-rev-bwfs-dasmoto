# NotebookLM URL Sender (Windows 11 + Chrome)

## Proposed architecture
- **Chrome MV3 extension** (`extension/`): popup UX, tab selection, cached notebooks, native messaging bridge.
- **Native host app** (`native-host/`): packaged .NET 8 executable, Chrome automation against NotebookLM using existing signed-in Chrome profile.
- **Automation selector notes** (`docs/SELECTOR_MAP.md`): primary + fallback selectors and error mappings.

## Chosen language/tooling for native host
- **C# / .NET 8** (no Node.js required on user machine).
- **PuppeteerSharp** for robust browser automation with explicit waits and fallback selectors.
- **PowerShell scripts** for build and host registration.

## Folder structure
- `extension/` MV3 extension source.
- `native-host/src/` native messaging and automation code.
- `native-host/manifest/` native messaging manifest template.
- `native-host/scripts/` build and registration scripts.
- `docs/` setup, troubleshooting, test plan, selector map.

## Setup steps
1. Install .NET 8 SDK **on build machine only**.
2. Build native host:
   ```powershell
   cd native-host/scripts
   .\build.ps1
   ```
3. Install host binary on target machine:
   - Copy `native-host/dist/NotebookLmSender.Host.exe` to `C:\Program Files\NotebookLmUrlSender\`.
4. Load extension:
   - Open `chrome://extensions`
   - Enable **Developer mode**
   - **Load unpacked** -> select `extension/`
   - Copy extension ID.
5. Register native host:
   ```powershell
   cd native-host/scripts
   .\register-host.ps1 -ExtensionId <your_extension_id>
   ```
6. Click extension icon and run flows.

## Implementation plan
- Phase 1: current tab + existing/new notebook + native messaging + packaged host + core errors.
- Phase 2: multi-select tabs, cached notebook refresh/recovery, per-URL failure summary, debug logs.
- Phase 3: installer polish, richer diagnostics and optional job history.

## Known risks
- NotebookLM DOM can change; fallback selectors may need updates.
- Using existing Chrome profile can fail when profile is locked by security software/heavy concurrent sessions.
- Notebook list scraping currently name-based; stable notebook IDs are not guaranteed.

## Next manual test steps
- Follow `docs/TESTPLAN.md` top-to-bottom.
- Validate each required user-facing status text in popup.
- Run signed-out, limit-reached, and UI-change simulation cases.
