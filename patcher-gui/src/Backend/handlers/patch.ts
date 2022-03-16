import fs from 'fs';
import path from 'path';
import {
  isMac,
  getPathToTowerfallExe,
  execShellCommand,
  getPatchFilesPath,
} from '../utils';
import { Version } from '../../types';

export const checkPatchability = (
  event: Electron.IpcMainInvokeEvent,
  towerfallPath: string
) => {
  const pathToExe = getPathToTowerfallExe(towerfallPath);
  return {
    canPatch: fs.existsSync(path.join(pathToExe, 'Towerfall.exe')),
    canUnpatch: fs.existsSync(path.join(pathToExe, 'TowerFall-Original.exe')),
  };
};

export async function patchGame(
  event: Electron.IpcMainInvokeEvent,
  towerfallPath: string,
  towerfallVersion: Version
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
