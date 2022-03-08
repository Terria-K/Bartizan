import { dialog, BrowserWindow } from 'electron';
import fs from 'fs';
import { isMac } from './utils';

export const browseFiles = (window: BrowserWindow) => {
  return async () => {
    const { canceled, filePaths } = await dialog.showOpenDialog(window);
    if (canceled) {
      return;
    } else {
      return filePaths[0];
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
