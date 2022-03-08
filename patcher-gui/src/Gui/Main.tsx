import React, { useState } from 'react';
import { version as appVersion } from '../../package.json';
import VersionSelector, { Version } from './VersionSelector';
import FileBrowser from './FileBrowser';

const Main: React.FC = () => {
  const [towerfallVersion, setTowerfallVersion] = useState<Version>(null);

  return (
    <div>
      <h1>Bartizan (Plus) Patcher</h1>
      <h4>Version: {appVersion}</h4>
      <VersionSelector onChange={setTowerfallVersion} />
      <FileBrowser version={towerfallVersion} />
    </div>
  );
};

export default Main;
