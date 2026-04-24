# Selector Map / Automation Notes

## Notebook home
- URL: `https://notebooklm.google.com`
- Notebook cards/links: `a[href*='/notebook/']`

## Create notebook
- Primary: `button[aria-label*='Create']`
- Fallback: `button:has-text('Create')`
- Input action: type notebook name + Enter

## Add source
- Primary: `button[aria-label*='Add source']`
- Fallback: `button:has-text('Add source')`
- Source type primary: `button:has-text('Web URL')`
- Source type fallback: `button[aria-label*='Web URL']`

## Error text parsing
- notebook limit: contains `100 notebooks` or `notebook limit`
- source limit: contains `50 sources` or `source limit`
- session expired: contains `Sign in`/Google account flow

## Reliability notes
- Explicit waits before click/type.
- Retry through fallback selectors.
- If no selectors match, return `AUTOMATION_UI_CHANGED`.
