import { contextBridge, ipcRenderer } from 'electron';
import ipcEvents from './IpcEvents';
import { Version } from './types';

contextBridge.exposeInMainWorld('api', {
  browseFiles: () => ipcRenderer.invoke(ipcEvents.BROWSE_FILES),
  checkForDefaultInstallation: (towerfallVersion: Version) =>
    ipcRenderer.invoke(
      ipcEvents.CHECK_FOR_DEFAULT_INSTALLATION,
      towerfallVersion
    ),
  checkPatchability: (towerfallPath: string) =>
    ipcRenderer.invoke(ipcEvents.CHECK_PATCHABILITY, towerfallPath),
  patch: (towerfallPath: string, towerfallVersion: Version) =>
    ipcRenderer.invoke(ipcEvents.PATCH, towerfallPath, towerfallVersion),
  unpatch: (towerfallPath: string) =>
    ipcRenderer.invoke(ipcEvents.UNPATCH, towerfallPath),
});
