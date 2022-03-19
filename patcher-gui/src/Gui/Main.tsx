import React, { useState, useEffect } from 'react';
import { version as appVersion } from '../../package.json';
import VersionSelector from './VersionSelector';
import FileBrowser from './FileBrowser';
import ActionButtons from './ActionButtons';
import Alert from './general/Alert';
import type { AlertColor } from '@mui/material';
import { Version } from '../types';

const Main: React.FC = () => {
  const [towerfallVersion, setTowerfallVersion] = useState<Version>(null);
  const [towerfallPath, setTowerfallPath] = useState<string>(null);
  const [alert, setAlert] =
    useState<{ message: string; color: AlertColor }>(null);

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
        onPatchSuccess={() =>
          setAlert({ message: 'Patch Successful', color: 'success' })
        }
        onPatchFail={() =>
          setAlert({ message: 'Patch Failed', color: 'error' })
        }
        onUnpatchSuccess={() =>
          setAlert({
            message: 'Unpatch Successful',
            color: 'success',
          })
        }
        onUnpatchFail={() =>
          setAlert({
            message: 'Unpatch Failed',
            color: 'success',
          })
        }
      />
      <Alert alert={alert} onClose={() => setAlert(null)} />
    </div>
  );
};

export default Main;
