import { contextBridge, ipcRenderer } from 'electron';
import ipcEvents from './IpcEvents';

contextBridge.exposeInMainWorld('api', {
  browseFiles: () => ipcRenderer.invoke(ipcEvents.BROWSE_FILES),
  checkForDefaultInstallation: () =>
    ipcRenderer.invoke(ipcEvents.CHECK_FOR_DEFAULT_INSTALLATION),
});
