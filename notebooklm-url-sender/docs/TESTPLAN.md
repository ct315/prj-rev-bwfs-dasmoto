# Basic Test Plan

## Preconditions
- Extension loaded unpacked.
- Native host built, installed, and registered.
- Chrome default profile signed into Google and can open NotebookLM.

## Cases
1. Add current tab URL to existing notebook.
2. Add current tab URL to new notebook.
3. Cancel/blank new notebook name -> expect `Notebook creation cancelled`.
4. Select multiple tabs -> verify order matches tab strip order.
5. Include duplicate URLs -> duplicates skipped.
6. Include invalid URLs (`chrome://`, `chrome-extension://`) -> skipped.
7. Stale notebook cache -> refresh and retry.
8. Source limit reached -> expect `Source limit reached`, stop immediately.
9. Notebook limit reached on create -> expect `Notebook limit reached`, stop immediately.
10. Sign out Google -> expect `Google session expired`.
11. Trigger UI selector break -> expect `NotebookLM page changed, automation needs update`.
12. Multi-add with one bad URL import -> record failed URL and continue.

## Pass criteria
- Correct popup status strings.
- Structured native host JSON result shape.
- Logs generated with failure detail.
