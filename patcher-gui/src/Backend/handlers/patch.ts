import fs from 'fs';
import path from 'path';
import {
  isMac,
  getPathToTowerfallExe,
  execShellCommand,
  getPatchFilesPath,
  unlinkIfExists,
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

export async function unpatchGame(
  event: Electron.IpcMainInvokeEvent,
  towerfallPath: string
): Promise<boolean> {
  try {
    const pathToExe = getPathToTowerfallExe(towerfallPath);
    if (pathToExe) {
      if (fs.existsSync(path.join(pathToExe, 'TowerFall-Original.exe'))) {
        fs.renameSync(
          path.join(pathToExe, 'TowerFall-Original.exe'),
          path.join(pathToExe, 'TowerFall.exe')
        );
        unlinkIfExists(path.join(pathToExe, 'Mod.dll'));
        unlinkIfExists(
          path.join(pathToExe, 'Content', 'Atlas', 'modAtlas.xml')
        );
        unlinkIfExists(
          path.join(pathToExe, 'Content', 'Atlas', 'modAtlas.png')
        );
        return Promise.resolve(true);
      }
    }
    return Promise.resolve(false);
  } catch (error) {
    return Promise.resolve(false);
  }
}

export async function patchGame(
  event: Electron.IpcMainInvokeEvent,
  towerfallPath: string,
  towerfallVersion: Version
): Promise<boolean> {
  try {
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
      const command = isMac()
        ? 'mono'
        : path.join(patchFilesPath, 'Patcher.exe');
      const args = [];
      if (isMac()) {
        args.push(path.join(patchFilesPath, 'Patcher.exe'));
      }
      args.push.apply(args, [
        'patch-exe',
        path.join(patchFilesPath, `Mod-${towerfallVersion}.dll`),
        pathToExe,
        path.join(pathToExe, 'TowerFall.exe'),
      ]);
      execShellCommand(command, args);
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
      return Promise.resolve(true);
    }
    return Promise.resolve(false);
  } catch (error) {
    return Promise.resolve(false);
  }
}
