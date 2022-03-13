import React, { useEffect, useState } from 'react';
import type { Version } from '../VersionSelector';
import InstallationSelector from './InstallationSelector';
import { usePrevious } from '../utils';
import Button from '../general/Button';
import testIds from '../test-helpers/testIds';

type FileBrowserProps = {
  version: Version;
  onChange: (towerfallPath: string) => void;
};

type Installation = 'default' | 'other';

const FileBrowser: React.FC<FileBrowserProps> = ({ version, onChange }) => {
  const [installation, setInstallation] = useState<Installation>(null);
  const [defaultInstallationPath, setDefaultInstallationPath] =
    useState<string>(null);
  const [otherInstallationPath, setOtherInstallationPath] =
    useState<string>(null);

  const previousVersion = usePrevious(version);

  useEffect(() => {
    if (version === '4-player' && previousVersion !== '4-player') {
      window.api.checkForDefaultInstallation().then((path: string) => {
        setDefaultInstallationPath(path);
        if (typeof path === 'string' && installation === 'default') {
          onChange(path);
        } else if (installation === 'other' && otherInstallationPath) {
          onChange(otherInstallationPath);
        }
      });
    } else if (!version) {
      onChange(null);
    }
  }, [installation, onChange, otherInstallationPath, previousVersion, version]);

  useEffect(() => {
    if (installation === 'default' && defaultInstallationPath) {
      onChange(defaultInstallationPath);
    } else if (installation === 'other') {
      onChange(otherInstallationPath);
    }
  }, [defaultInstallationPath, installation, onChange, otherInstallationPath]);

  if (!version) {
    return null;
  }

  return (
    <div>
      <InstallationSelector
        version={version}
        defaultInstallationPath={defaultInstallationPath}
        installation={installation}
        onChange={setInstallation}
      />
      {version === '8-player' ||
      (version === '4-player' && installation === 'other') ||
      (version === '4-player' && defaultInstallationPath === null) ? (
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
