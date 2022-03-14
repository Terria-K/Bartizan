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

const getPatchFilesPath = () => {
  if (process.env.NODE_ENV === 'development') {
    return path.normalize(path.join(__dirname, '..', '..', 'patchFiles'));
  } else {
    return process.resourcesPath;
  }
};

export async function patchGame(
  towerfallPath: string,
  towerfallVersion: string
): Promise<boolean> {
  const patchFilesPath = getPatchFilesPath();
  const pathToExe = getPathToTowerfallExe(towerfallPath);
  if (pathToExe) {
    // If backup exists, restore the original TowerFall.exe before patching
    if (fs.existsSync(path.join(pathToExe, 'TowerFall-Original.exe'))) {
      fs.copyFileSync(
        path.join(pathToExe, 'TowerFall-Original.exe'),
        path.join(pathToExe, 'TowerFall.exe')
      );
    } else {
      fs.copyFileSync(
        path.join(pathToExe, 'TowerFall.exe'),
        path.join(pathToExe, 'TowerFall-Original.exe')
      );
    }
    // Patch EXE
    const commandPrefix = isMac() ? 'mono ' : '';
    const patcherPath = path.join(patchFilesPath, 'Patcher.exe');
    const patcherArgs = [
      `Mod-${towerfallVersion}.dll`,
      pathToExe,
      path.join(pathToExe, 'TowerFall.exe'),
    ];
    await execShellCommand(
      `${commandPrefix}${patcherPath} ${patcherArgs.join(' ')}`
    );
    // Copy Mod.dll file
    fs.copyFileSync(
      path.join(patchFilesPath, `Mod-${towerfallVersion}.dll`),
      path.join(pathToExe, 'Mod.dll')
    );
    // Copy atlas files
    fs.copyFileSync(
      path.join(patchFilesPath, `modAtlas.xml`),
      path.join(pathToExe, 'Content', 'Atlas', 'modAtlas.xml')
    );
    fs.copyFileSync(
      path.join(patchFilesPath, `modAtlas.png`),
      path.join(pathToExe, 'Content', 'Atlas', 'modAtlas.png')
    );
  } else {
    return Promise.resolve(false);
  }

  return execShellCommand('ls');
}

export function execShellCommand(cmd: string): Promise<boolean> {
  return new Promise((resolve, reject) => {
    exec(cmd, { maxBuffer: 1024 * 500 }, (error, stdout, stderr) => {
      if (error) {
        console.warn(error);
        reject();
      } else if (stdout) {
        resolve(true);
      } else {
        console.log(stderr);
        reject();
      }
    });
  });
}
