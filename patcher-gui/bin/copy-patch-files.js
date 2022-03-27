#!/usr/bin/env node
/* eslint-disable @typescript-eslint/no-var-requires */
const fs = require('fs');
const path = require('path');

const source = path.normalize(path.join(__dirname, '..', '..', 'bin'));
const destination = path.normalize(path.join(__dirname, '..', 'patchFiles'));

const isMac = process.platform === 'darwin';

if (!fs.existsSync(destination)) {
  fs.mkdirSync(destination);
}

fs.copyFileSync(
  path.join(source, 'Patcher.exe'),
  path.join(destination, 'Patcher.exe')
);
fs.copyFileSync(
  path.join(source, 'Content', 'Atlas', 'modAtlas.png'),
  path.join(destination, 'modAtlas.png')
);
fs.copyFileSync(
  path.join(source, 'Content', 'Atlas', 'modAtlas.xml'),
  path.join(destination, 'modAtlas.xml')
);
if (isMac) {
  fs.copyFileSync(
    path.join(source, 'System.Xml.Linq.dll'),
    path.join(destination, 'System.Xml.Linq.dll')
  );
} else {
  fs.copyFileSync(
    path.join(source, 'Mono.Cecil.dll'),
    path.join(destination, 'Mono.Cecil.dll')
  );
}
fs.copyFileSync(
  path.join(source, 'builds', '4-player', 'Mod.dll'),
  path.join(destination, 'Mod-4-player-steam.dll')
);
fs.copyFileSync(
  path.join(source, 'builds', '4-player-itch', 'Mod.dll'),
  path.join(destination, 'Mod-4-player-itch.dll')
);
fs.copyFileSync(
  path.join(source, 'builds', '8-player', 'Mod.dll'),
  path.join(destination, 'Mod-8-player.dll')
);
