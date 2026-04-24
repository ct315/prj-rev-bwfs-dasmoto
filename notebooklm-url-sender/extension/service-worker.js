import { saveNotebookCache, setAppStatus } from "./storage.js";
import { sendToNativeHost } from "./messaging.js";

chrome.runtime.onMessage.addListener((message, _sender, sendResponse) => {
  if (message?.type === "nativeJob") {
    sendToNativeHost(message.payload)
      .then(async (result) => {
        if (Array.isArray(result?.notebooks)) {
          await saveNotebookCache(result.notebooks);
        }
        await setAppStatus({ lastRunAt: new Date().toISOString(), result });
        sendResponse(result);
      })
      .catch(async (error) => {
        const failure = {
          ok: false,
          errorCode: "NATIVE_MESSAGING_FAILED",
          message: error.message
        };
        await setAppStatus({ lastRunAt: new Date().toISOString(), result: failure });
        sendResponse(failure);
      });
    return true;
  }

  return false;
});
