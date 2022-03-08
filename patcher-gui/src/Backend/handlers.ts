import { dialog, BrowserWindow } from 'electron';

export const handleBrowseFiles = (window: BrowserWindow) => {
  return async () => {
    const { canceled, filePaths } = await dialog.showOpenDialog(window);
    if (canceled) {
      return;
    } else {
      return filePaths[0];
    }
  };
};
