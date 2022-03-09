import { dialog, BrowserWindow, OpenDialogOptions } from 'electron';
import fs from 'fs';
import { isMac } from './utils';

const isTowerFallPathValid = (path: string) => {
  if (isMac()) {
    if (fs.existsSync(`${path}/Contents/Resources/TowerFall.exe`)) {
      return true;
    } else if (fs.existsSync(`${path}/Contents/MacOS/TowerFall.exe`)) {
      return true;
    } else {
      return false;
    }
  }

  return fs.existsSync(String.raw`${path}\TowerFall.exe`);
};

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

export const checkForDefaultInstallation = () => {
  if (isMac()) {
    const home = process.env.HOME;
    const defaultPath = `${home}/Library/Application Support/Steam/steamapps/common/TowerFall/TowerFall.app`;
    const pathToExe = `${defaultPath}/Contents/Resources/TowerFall.exe`;
    if (fs.existsSync(pathToExe)) {
      return defaultPath;
    }
  } else {
    const programFiles = process.env['ProgramFiles(x86)'];
    const defaultPath = String.raw`${programFiles}\Steam\SteamApps\common\TowerFall`;
    const pathToExe = String.raw`${defaultPath}\TowerFall.exe`;
    if (fs.existsSync(pathToExe)) {
      return defaultPath;
    }
  }
  return null;
};
