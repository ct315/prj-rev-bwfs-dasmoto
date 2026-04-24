const STORAGE_KEYS = {
  NOTEBOOK_CACHE: "notebookCache",
  LAST_NOTEBOOK: "lastNotebook",
  APP_STATUS: "appStatus",
  DEBUG_MODE: "debugMode"
};

export async function getStoredState() {
  return chrome.storage.local.get(Object.values(STORAGE_KEYS));
}

export async function saveNotebookCache(cache) {
  return chrome.storage.local.set({
    [STORAGE_KEYS.NOTEBOOK_CACHE]: {
      items: cache,
      refreshedAt: new Date().toISOString()
    }
  });
}

export async function setLastNotebook(name) {
  return chrome.storage.local.set({ [STORAGE_KEYS.LAST_NOTEBOOK]: name });
}

export async function setAppStatus(status) {
  return chrome.storage.local.set({ [STORAGE_KEYS.APP_STATUS]: status });
}

export { STORAGE_KEYS };
