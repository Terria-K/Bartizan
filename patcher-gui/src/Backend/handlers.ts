import { dialog, BrowserWindow, OpenDialogOptions } from 'electron';
import fs from 'fs';
import path from 'path';
import { isMac } from './utils';

const getPathToTowerfallExeInMacPackage = (towerfallPath: string) => {
  if (fs.existsSync(`${towerfallPath}/Contents/Resources/TowerFall.exe`)) {
    return `${towerfallPath}/Contents/Resources/`;
  } else if (fs.existsSync(`${towerfallPath}/Contents/MacOS/TowerFall.exe`)) {
    return `${towerfallPath}/Contents/MacOS/`;
  }
  return '';
};

const isTowerFallPathValid = (towerfallPath: string) => {
  if (isMac()) {
    return !!getPathToTowerfallExeInMacPackage(towerfallPath);
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
  return '';
};

export const checkPatchability = (
  event: Electron.IpcMainInvokeEvent,
  towerfallPath: string
) => {
  const pathToExe = isMac()
    ? getPathToTowerfallExeInMacPackage(towerfallPath)
    : towerfallPath;
  return {
    canPatch: fs.existsSync(path.join(pathToExe, 'Towerfall.exe')),
    canUnpatch: fs.existsSync(path.join(pathToExe, 'TowerFall-Original.exe')),
  };
};
