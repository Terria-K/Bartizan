#!/usr/bin/env node
/* eslint-disable @typescript-eslint/no-var-requires */
const fs = require('fs');
const path = require('path');

const source = path.normalize(path.join(__dirname, '..', '..', 'bin'));
const destination = path.normalize(path.join(__dirname, '..', 'patchFiles'));

if (!fs.existsSync(destination)) {
  fs.mkdirSync(destination);
}

const copyFromBin = [
  'Mono.Cecil.dll',
  'Mono.Cecil.Mdb.dll',
  'Mono.Cecil.Pdb.dll',
  'MonoMod.Utils.dll',
  'MonoMod.exe',
];

copyFromBin.forEach(filename => {
  fs.copyFileSync(
    path.join(source, filename),
    path.join(destination, filename)
  );
});

fs.copyFileSync(
  path.join(source, 'Content', 'Atlas', 'modAtlas.png'),
  path.join(destination, 'modAtlas.png')
);
fs.copyFileSync(
  path.join(source, 'Content', 'Atlas', 'modAtlas.xml'),
  path.join(destination, 'modAtlas.xml')
);

fs.copyFileSync(
  path.join(source, 'builds', '4-player', 'TowerFall.Mod.mm.dll'),
  path.join(destination, 'TowerFall.4-player-steam.mm.dll')
);
fs.copyFileSync(
  path.join(source, 'builds', '4-player-itch', 'TowerFall.Mod.mm.dll'),
  path.join(destination, 'TowerFall.4-player-itch.mm.dll')
);
fs.copyFileSync(
  path.join(source, 'builds', '8-player', 'TowerFall.Mod.mm.dll'),
  path.join(destination, 'TowerFall.8-player.mm.dll')
);
