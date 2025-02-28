import { spawnSync } from 'child_process';
import path from 'path';
import fs from 'fs';
import fixPath from 'fix-path';

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
    return path.join(process.resourcesPath, 'patchFiles');
  }
};

export function execShellCommand(cmd: string, args: string[] = [], cwd?: string): Buffer {
  fixPath();
  const process = spawnSync(cmd, args, cwd ? { cwd } : null);
  if (process.error) {
    throw process.error;
  }
  return process.stdout;
}

export function unlinkIfExists(path: string) {
  if (fs.existsSync(path)) {
    return fs.unlinkSync(path);
  }
}

export function escapeFilePath(filePath: string): string {
  return `"${filePath.replace('"', String.raw`\"`)}"`;
}
