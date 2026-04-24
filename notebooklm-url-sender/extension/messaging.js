const HOST_NAME = "com.example.notebooklm_sender";

export function sendToNativeHost(payload) {
  return new Promise((resolve, reject) => {
    chrome.runtime.sendNativeMessage(HOST_NAME, payload, (response) => {
      if (chrome.runtime.lastError) {
        reject(new Error(chrome.runtime.lastError.message));
        return;
      }
      resolve(response);
    });
  });
}

export function toUserMessage(result) {
  if (!result) {
    return "Could not add URL source";
  }

  if (result.ok) {
    return "Added source successfully";
  }

  switch (result.errorCode) {
    case "NOTEBOOK_NOT_FOUND":
      return "Could not find selected notebook";
    case "NOTEBOOK_LIMIT_REACHED":
      return "Notebook limit reached";
    case "SOURCE_LIMIT_REACHED":
      return "Source limit reached";
    case "SESSION_EXPIRED":
      return "Google session expired";
    case "AUTOMATION_UI_CHANGED":
      return "NotebookLM page changed, automation needs update";
    default:
      return result.message || "Could not add URL source";
  }
}
