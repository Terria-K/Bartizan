import React, { useState, useEffect } from 'react';
import { version as appVersion } from '../../package.json';
import VersionSelector, { Version } from './VersionSelector';
import FileBrowser from './FileBrowser';
import ActionButtons from './ActionButtons';

const Main: React.FC = () => {
  const [towerfallVersion, setTowerfallVersion] = useState<Version>(null);
  const [towerfallPath, setTowerfallPath] = useState<string>(null);

  useEffect(() => {
    setTowerfallPath(null);
  }, [towerfallVersion]);

  return (
    <div>
      <h1>Bartizan (Plus) Patcher</h1>
      <h4>Version: {appVersion}</h4>
      <VersionSelector onChange={setTowerfallVersion} />
      <FileBrowser version={towerfallVersion} onChange={setTowerfallPath} />
      <ActionButtons towerfallPath={towerfallPath} />
    </div>
  );
};

export default Main;
