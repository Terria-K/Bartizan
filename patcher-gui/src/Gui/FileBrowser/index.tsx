import React, { useEffect, useState } from 'react';
import InstallationSelector from './InstallationSelector';
import { usePrevious } from '../utils';
import Button from '../general/Button';
import testIds from '../test-helpers/testIds';
import { Version } from '../../types';

type FileBrowserProps = {
  towerfallVersion: Version;
  onChange: (towerfallPath: string) => void;
};

type Installation = 'default' | 'other';

const FileBrowser: React.FC<FileBrowserProps> = ({
  towerfallVersion,
  onChange,
}) => {
  const [installation, setInstallation] = useState<Installation>(null);
  const [defaultInstallationPath, setDefaultInstallationPath] =
    useState<string>(null);
  const [otherInstallationPath, setOtherInstallationPath] =
    useState<string>(null);

  const previousVersion: Version = usePrevious<Version>(towerfallVersion);

  useEffect(() => {
    if (towerfallVersion !== previousVersion) {
      setInstallation(null);
      setDefaultInstallationPath(null);
      setOtherInstallationPath(null);
      onChange(null);
      if (towerfallVersion?.startsWith('4-player')) {
        window.api
          .checkForDefaultInstallation(towerfallVersion)
          .then((path: string) => {
            setDefaultInstallationPath(path);
          });
      }
    }
  }, [
    installation,
    onChange,
    otherInstallationPath,
    previousVersion,
    towerfallVersion,
  ]);

  useEffect(() => {
    if (installation === 'default' && defaultInstallationPath) {
      onChange(defaultInstallationPath);
    } else if (installation === 'other') {
      onChange(otherInstallationPath);
    }
  }, [defaultInstallationPath, installation, onChange, otherInstallationPath]);

  if (!towerfallVersion) {
    return null;
  }

  return (
    <div>
      <InstallationSelector
        towerfallVersion={towerfallVersion}
        defaultInstallationPath={defaultInstallationPath}
        installation={installation}
        onChange={setInstallation}
      />
      {towerfallVersion === '8-player' ||
      (towerfallVersion.startsWith('4-player') && installation === 'other') ||
      (towerfallVersion.startsWith('4-player') &&
        defaultInstallationPath === '') ? (
        <div>
          <h3>Locate the TowerFall installation you wish to patch:</h3>
          <Button
            onClick={async () => {
              const file = await window.api.browseFiles();
              onChange(file);
              setOtherInstallationPath(file);
            }}
            data-testid={testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON}
          >
            Browse
          </Button>
        </div>
      ) : null}
    </div>
  );
};

export default FileBrowser;
