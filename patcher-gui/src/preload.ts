import { contextBridge, ipcRenderer } from 'electron';
import ipcEvents from './IpcEvents';

contextBridge.exposeInMainWorld('api', {
  browseFiles: () => ipcRenderer.invoke(ipcEvents.BROWSE_FILES),
  checkForDefaultInstallation: () =>
    ipcRenderer.invoke(ipcEvents.CHECK_FOR_DEFAULT_INSTALLATION),
  checkPatchability: (towerfallPath: string) =>
    ipcRenderer.invoke(ipcEvents.CHECK_PATCHABILITY, towerfallPath),
  patch: (towerfallPath: string, towerfallVersion: string) =>
    ipcRenderer.invoke(ipcEvents.PATCH, towerfallPath, towerfallVersion),
});
