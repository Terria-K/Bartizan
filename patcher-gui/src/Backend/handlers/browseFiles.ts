import { dialog, BrowserWindow, OpenDialogOptions } from 'electron';
import { isMac, isTowerFallPathValid } from '../utils';

export const browseFiles = (window: BrowserWindow) => {
  const dialogOptions = isMac()
    ? {
        filters: [
          {
            name: 'App',
            extensions: ['app'],
          },
        ],
        message: 'Locate TowerFall.app',
        properties: [
          'openFile',
          'showHiddenFiles',
        ] as OpenDialogOptions['properties'],
      }
    : {
        message: 'Locate TowerFall directory',
        properties: ['openDirectory'] as OpenDialogOptions['properties'],
      };

  return async () => {
    const { canceled, filePaths } = await dialog.showOpenDialog(
      window,
      dialogOptions
    );
    if (canceled) {
      return;
    } else {
      if (isTowerFallPathValid(filePaths[0])) {
        return filePaths[0];
      } else {
        dialog.showErrorBox('Error', `Can't find TowerFall there`);
      }
    }
  };
};
