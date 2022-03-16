import React, { useState, useEffect } from 'react';
import { version as appVersion } from '../../package.json';
import VersionSelector from './VersionSelector';
import FileBrowser from './FileBrowser';
import ActionButtons from './ActionButtons';
import { Version } from '../types';

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
      <VersionSelector
        towerfallVersion={towerfallVersion}
        onChange={setTowerfallVersion}
      />
      <FileBrowser
        towerfallVersion={towerfallVersion}
        onChange={setTowerfallPath}
      />
      <ActionButtons
        towerfallPath={towerfallPath}
        towerfallVersion={towerfallVersion}
      />
    </div>
  );
};

export default Main;
