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

const patchFilesTargetDir = 'Bartizan-Plus'
const targetModDllFilename = 'TowerFall.Mod.mm.dll';

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
  towerfallPath: string,
  towerfallVersion: Version,
): Promise<boolean> {
  try {
    const pathToExe = getPathToTowerfallExe(towerfallPath);
    if (pathToExe) {
      if (fs.existsSync(path.join(pathToExe, 'TowerFall-Original.exe'))) {
        fs.renameSync(
          path.join(pathToExe, 'TowerFall-Original.exe'),
          path.join(pathToExe, 'TowerFall.exe')
        );
        unlinkIfExists(path.join(pathToExe, 'Mod.dll')); // If left over from v1 patch
        unlinkIfExists(
          path.join(pathToExe, 'Content', 'Atlas', 'modAtlas.xml')
        );
        unlinkIfExists(
          path.join(pathToExe, 'Content', 'Atlas', 'modAtlas.png')
        );
        getFilesToCopyFromExePath(towerfallVersion).forEach(filename => {
          unlinkIfExists(path.join(pathToExe, patchFilesTargetDir, filename));
        });
        getFilesToCopyFromPatchFiles().forEach(filename => {
          unlinkIfExists(path.join(pathToExe, patchFilesTargetDir, filename));
        });
        unlinkIfExists(path.join(pathToExe, patchFilesTargetDir, targetModDllFilename));
        unlinkIfExists(path.join(pathToExe, patchFilesTargetDir, 'MONOMODDED_TowerFall.exe'));
        unlinkIfExists(path.join(pathToExe, patchFilesTargetDir, 'MONOMODDED_TowerFall.exe.mdb'));
        try {
          fs.rmdirSync(path.join(pathToExe, patchFilesTargetDir));
        } catch (error) {
          // Fine if patch files target path not removed
          return Promise.resolve(true);
        }
        return Promise.resolve(true);
      }
    }
    return Promise.resolve(false);
  } catch (error) {
    return Promise.resolve(false);
  }
}

function getFilesToCopyFromPatchFiles(): string[] {
  return [
    'Mono.Cecil.dll',
    'Mono.Cecil.Mdb.dll',
    'Mono.Cecil.Pdb.dll',
    'MonoMod.Utils.dll',
    'MonoMod.exe',
  ];
}

function getFilesToCopyFromExePath(towerfallVersion: Version): string[] {
  const files = [
    'TowerFall.exe',
    'FNA.dll',
  ];
  if (towerfallVersion === '4-player-steam') {
    files.push('Steamworks.NET.dll');
  }
  return files;
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

      const filesToCopyFromExePath = getFilesToCopyFromExePath(towerfallVersion);
      const filesToCopyFromPatchFiles = getFilesToCopyFromPatchFiles();

      if (!fs.existsSync(path.join(pathToExe, patchFilesTargetDir))) {
        fs.mkdirSync(path.join(pathToExe, patchFilesTargetDir));
      }

      filesToCopyFromExePath.forEach(filename => {
        fs.copyFileSync(
          path.join(pathToExe, filename),
          path.join(pathToExe, patchFilesTargetDir, filename)
        );
      });

      filesToCopyFromPatchFiles.forEach(filename => {
        fs.copyFileSync(
          path.join(patchFilesPath, filename),
          path.join(pathToExe, patchFilesTargetDir, filename)
        );
      });
      // Copy dll file for the selected TF version
      fs.copyFileSync(
        path.join(patchFilesPath, `TowerFall.${towerfallVersion}.mm.dll`),
        path.join(pathToExe, patchFilesTargetDir, targetModDllFilename)
      );
      // Patch EXE
      const command = isMac()
        ? 'mono'
        : 'MonoMod.exe';
      const args: string[] = [];
      if (isMac()) {
        args.push(path.join(pathToExe, patchFilesTargetDir, 'MonoMod.exe'));
      }
      args.push.apply(args, [
        path.join('TowerFall.exe'),
      ]);
      execShellCommand(command, args, path.join(pathToExe, patchFilesTargetDir));
      fs.copyFileSync(
        path.join(pathToExe, patchFilesTargetDir, 'MONOMODDED_TowerFall.exe'),
        path.join(pathToExe, 'TowerFall.exe')
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
