import { getStoredState, saveNotebookCache, setLastNotebook } from "./storage.js";
import { toUserMessage } from "./messaging.js";

const els = {
  statusArea: document.getElementById("statusArea"),
  notebookPicker: document.getElementById("notebookPicker"),
  refreshNotebooksBtn: document.getElementById("refreshNotebooksBtn"),
  submitBtn: document.getElementById("submitBtn"),
  existingNotebookSection: document.getElementById("existingNotebookSection"),
  newNotebookSection: document.getElementById("newNotebookSection"),
  newNotebookName: document.getElementById("newNotebookName"),
  tabSelectionSection: document.getElementById("tabSelectionSection"),
  tabSelectionList: document.getElementById("tabSelectionList")
};

let currentWindowTabs = [];

function setStatus(text) {
  els.statusArea.textContent = text;
}

function isValidWebUrl(url) {
  if (!url || typeof url !== "string") return false;
  if (url.startsWith("chrome://") || url.startsWith("chrome-extension://")) return false;
  try {
    const parsed = new URL(url);
    return ["http:", "https:"].includes(parsed.protocol);
  } catch {
    return false;
  }
}

async function loadTabs() {
  const tabs = await chrome.tabs.query({ currentWindow: true });
  currentWindowTabs = tabs.sort((a, b) => a.index - b.index);
  renderTabSelection();
}

function renderTabSelection() {
  els.tabSelectionList.innerHTML = "";
  for (const tab of currentWindowTabs) {
    const row = document.createElement("label");
    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.value = tab.id;
    const text = document.createTextNode(` ${tab.title || tab.url}`);
    row.appendChild(checkbox);
    row.appendChild(text);
    els.tabSelectionList.appendChild(row);
  }
}

function renderNotebookPicker(notebooks, lastNotebook) {
  els.notebookPicker.innerHTML = "";
  if (!notebooks || notebooks.length === 0) {
    const option = document.createElement("option");
    option.value = "";
    option.textContent = "No cached notebooks. Click refresh.";
    els.notebookPicker.appendChild(option);
    return;
  }

  for (const notebook of notebooks) {
    const option = document.createElement("option");
    option.value = notebook.name;
    option.textContent = notebook.name;
    if (notebook.name === lastNotebook) option.selected = true;
    els.notebookPicker.appendChild(option);
  }
}

function getSelectedTabMode() {
  return document.querySelector('input[name="tabMode"]:checked').value;
}

function getSelectedTargetMode() {
  return document.querySelector('input[name="targetMode"]:checked').value;
}

function collectUrls(tabMode) {
  let candidateTabs = [];
  if (tabMode === "current") {
    const selected = currentWindowTabs.find((t) => t.active);
    candidateTabs = selected ? [selected] : [];
  } else if (tabMode === "all") {
    candidateTabs = currentWindowTabs;
  } else {
    const selectedIds = [...els.tabSelectionList.querySelectorAll('input[type="checkbox"]:checked')].map((cb) => Number(cb.value));
    candidateTabs = currentWindowTabs.filter((tab) => selectedIds.includes(tab.id));
  }

  const seen = new Set();
  const urls = [];
  for (const tab of candidateTabs) {
    const url = tab.url?.trim();
    if (!isValidWebUrl(url) || seen.has(url)) continue;
    seen.add(url);
    urls.push(url);
  }

  return urls;
}

async function dispatchNativeJob(payload) {
  return chrome.runtime.sendMessage({ type: "nativeJob", payload });
}

async function refreshNotebooks() {
  setStatus("Refreshing notebooks...");
  const result = await dispatchNativeJob({ action: "list_notebooks" });
  if (result?.ok && Array.isArray(result.notebooks)) {
    await saveNotebookCache(result.notebooks);
    const state = await getStoredState();
    renderNotebookPicker(result.notebooks, state.lastNotebook);
    setStatus(`Loaded ${result.notebooks.length} notebook(s)`);
    return;
  }
  setStatus(toUserMessage(result));
}

function bindModeToggles() {
  document.querySelectorAll('input[name="targetMode"]').forEach((el) => {
    el.addEventListener("change", () => {
      const targetMode = getSelectedTargetMode();
      els.existingNotebookSection.hidden = targetMode !== "existing";
      els.newNotebookSection.hidden = targetMode !== "new";
    });
  });

  document.querySelectorAll('input[name="tabMode"]').forEach((el) => {
    el.addEventListener("change", () => {
      els.tabSelectionSection.hidden = getSelectedTabMode() !== "selected";
    });
  });
}

async function init() {
  bindModeToggles();
  await loadTabs();

  const state = await getStoredState();
  renderNotebookPicker(state.notebookCache?.items || [], state.lastNotebook);
  setStatus("Ready");

  els.refreshNotebooksBtn.addEventListener("click", refreshNotebooks);

  els.submitBtn.addEventListener("click", async () => {
    const tabMode = getSelectedTabMode();
    const targetMode = getSelectedTargetMode();
    const urls = collectUrls(tabMode);

    if (urls.length === 0) {
      setStatus("Could not add URL source");
      return;
    }

    let notebookName = null;
    if (targetMode === "existing") {
      notebookName = els.notebookPicker.value;
      if (!notebookName) {
        setStatus("Could not find selected notebook");
        return;
      }
      await setLastNotebook(notebookName);
    } else {
      notebookName = els.newNotebookName.value.trim();
      if (!notebookName) {
        setStatus("Notebook creation cancelled");
        return;
      }
    }

    const payload = {
      action: targetMode === "new" ? "create_notebook_and_add_sources" : "add_sources",
      targetNotebook: targetMode === "existing" ? notebookName : undefined,
      newNotebookName: targetMode === "new" ? notebookName : undefined,
      urls,
      mode: tabMode
    };

    setStatus("Running...");
    const result = await dispatchNativeJob(payload);

    if (result?.ok) {
      const summary = ["Added source successfully"];
      if (result.added?.length) summary.push(`Added: ${result.added.length}`);
      if (result.failed?.length) {
        summary.push(`Failed: ${result.failed.length}`);
        for (const item of result.failed) {
          summary.push(`- ${item.url}: ${item.reason}`);
        }
      }
      setStatus(summary.join("\n"));
      return;
    }

    setStatus(toUserMessage(result));
  });
}

init().catch((error) => setStatus(`Could not add URL source\n${error.message}`));
