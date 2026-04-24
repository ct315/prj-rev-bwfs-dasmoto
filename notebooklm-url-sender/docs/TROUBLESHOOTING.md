# Troubleshooting

## Native host not found
- Symptom: popup fails immediately with native messaging error.
- Check registry key:
  `HKCU\Software\Google\Chrome\NativeMessagingHosts\com.example.notebooklm_sender`
- Re-run `native-host/scripts/register-host.ps1` with correct extension ID.

## Google session expired
- Message: `Google session expired`
- Fix: open NotebookLM manually in same Chrome profile and sign in.

## Notebook missing from stale cache
- Message: `Could not find selected notebook`
- Fix: click **Refresh notebooks** in popup and retry.

## Notebook/source limits
- `Notebook limit reached` during creation -> delete old notebooks and retry.
- `Source limit reached` while adding URLs -> remove sources or choose another notebook.

## UI changed
- Message: `NotebookLM page changed, automation needs update`
- Fix: update selectors in `docs/SELECTOR_MAP.md` and `native-host/src/notebooklm-automation.cs`.

## Profile lock or Chrome launch issue
- Close extra Chrome processes and retry.
- Ensure default profile exists at `%LOCALAPPDATA%\Google\Chrome\User Data\Default`.

## Logs
- Location: `%LOCALAPPDATA%\NotebookLmUrlSender\logs\host-YYYYMMDD.log`
- Contains timestamped step trace, selector attempts, and sanitized errors.
