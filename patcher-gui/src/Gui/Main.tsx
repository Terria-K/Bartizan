import React, { useState, useEffect } from 'react';
import { version as appVersion } from '../../package.json';
import VersionSelector, { Version } from './VersionSelector';
import FileBrowser from './FileBrowser';

const Main: React.FC = () => {
  const [towerfallVersion, setTowerfallVersion] = useState<Version>(null);
  const [defaultInstallationPath, setDefaultInstallationPath] = useState<
    string | boolean
  >(null);

  useEffect(() => {
    if (towerfallVersion === '4-player') {
      window.api
        .checkForDefaultInstallation()
        .then((path: string | boolean) => {
          setDefaultInstallationPath(path);
        });
    }
  }, [towerfallVersion]);

  return (
    <div>
      <h1>Bartizan (Plus) Patcher</h1>
      <h4>Version: {appVersion}</h4>
      <VersionSelector onChange={setTowerfallVersion} />
      {defaultInstallationPath && (
        <p>Found TowerFall installed at "{defaultInstallationPath}"</p>
      )}
      <FileBrowser version={towerfallVersion} />
    </div>
  );
};

export default Main;
