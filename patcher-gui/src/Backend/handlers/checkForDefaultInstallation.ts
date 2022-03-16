import fs from 'fs';
import { isMac } from '../utils';
import { Version } from '../../types';

export const checkForDefaultInstallation = (
  event: Electron.IpcMainInvokeEvent,
  towerfallVersion: Version
) => {
  if (isMac()) {
    if (towerfallVersion === '4-player-steam') {
      const home = process.env.HOME;
      const defaultPath = `${home}/Library/Application Support/Steam/steamapps/common/TowerFall/TowerFall.app`;
      const pathToExe = `${defaultPath}/Contents/Resources/TowerFall.exe`;
      if (fs.existsSync(pathToExe)) {
        return defaultPath;
      }
    } else if (towerfallVersion === '4-player-itch') {
      const home = process.env.HOME;
      const defaultPath = `${home}/Library/Application Support/itch/apps/towerfall/TowerFall.app`;
      const pathToExe = `${defaultPath}/Contents/MacOS/TowerFall.exe`;
      if (fs.existsSync(pathToExe)) {
        return defaultPath;
      }
    }
  } else {
    if (towerfallVersion === '4-player-steam') {
      const programFiles = process.env['ProgramFiles(x86)'];
      const defaultPath = String.raw`${programFiles}\Steam\SteamApps\common\TowerFall`;
      const pathToExe = String.raw`${defaultPath}\TowerFall.exe`;
      if (fs.existsSync(pathToExe)) {
        return defaultPath;
      }
    } else if (towerfallVersion === '4-player-itch') {
      const programFiles = process.env['HOMEPATH'];
      const defaultPath = String.raw`${programFiles}\AppData\Roaming\itch\apps\towerfall\TowerFallAscension_Itch`;
      const pathToExe = String.raw`${defaultPath}\TowerFall.exe`;
      if (fs.existsSync(pathToExe)) {
        return defaultPath;
      }
    }
  }
  return '';
};
