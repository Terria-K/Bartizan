import { exec } from 'child_process';

export function isMac() {
  return process.platform === 'darwin';
}

export function execShellCommand(cmd: string) {
  console.log('exec shell command', cmd);
  return new Promise((resolve, reject) => {
    exec(cmd, { maxBuffer: 1024 * 500 }, (error, stdout, stderr) => {
      console.log('DID exec it', { error, stdout, stderr });
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
