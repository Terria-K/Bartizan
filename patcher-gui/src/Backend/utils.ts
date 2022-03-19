import { exec } from 'child_process';
import path from 'path';
import fs from 'fs';

export function isMac() {
  return process.platform === 'darwin';
}

export function getPathToTowerfallExe(towerfallPath: string) {
  if (isMac()) {
    if (fs.existsSync(`${towerfallPath}/Contents/Resources/TowerFall.exe`)) {
      return `${towerfallPath}/Contents/Resources/`;
    } else if (fs.existsSync(`${towerfallPath}/Contents/MacOS/TowerFall.exe`)) {
      return `${towerfallPath}/Contents/MacOS/`;
    }
  } else if (fs.existsSync(String.raw`${towerfallPath}\TowerFall.exe`)) {
    return towerfallPath;
  }
  return '';
}

export const isTowerFallPathValid = (towerfallPath: string) => {
  return !!getPathToTowerfallExe(towerfallPath);
};

export const getPatchFilesPath = () => {
  if (process.env.NODE_ENV === 'development') {
    return path.normalize(path.join(__dirname, '..', '..', 'patchFiles'));
  } else {
    return process.resourcesPath;
  }
};

export function execShellCommand(cmd: string): Promise<boolean> {
  return new Promise((resolve, reject) => {
    try {
      exec(cmd, { maxBuffer: 1024 * 500 }, (error) => {
        if (error) {
          console.error(error);
          reject();
        }
        resolve(true);
      });
    } catch (error) {
      console.error(error);
      reject();
    }
  });
}
