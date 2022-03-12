import React, { useEffect, useState } from 'react';
import type { Version } from './VersionSelector';
import RadioButtons from './general/RadioButtons';
import { usePrevious } from './utils';
import testIds from './test-helpers/testIds';

type FileBrowserProps = {
  version: Version;
  onChange: (towerfallPath: string) => void;
};

type Installation = 'default' | 'other';

const FileBrowser: React.FC<FileBrowserProps> = ({ version, onChange }) => {
  const previousVersion = usePrevious(version);
  const [defaultInstallationPath, setDefaultInstallationPath] = useState<
    string | boolean
  >(null);
  const [otherInstallationPath, setOtherInstallationPath] =
    useState<string>(null);
  const [installation, setInstallation] = useState<Installation>('default');

  useEffect(() => {
    if (version === '4-player' && previousVersion !== '4-player') {
      window.api
        .checkForDefaultInstallation()
        .then((path: string | boolean) => {
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

  if (!version) {
    return null;
  }

  return (
    <div>
      {version === '4-player' && !!defaultInstallationPath && (
        <div>
          <p>Found TowerFall installed at {defaultInstallationPath}</p>
          <RadioButtons<Installation>
            options={[
              {
                label: 'Select this installation',
                value: 'default',
                testID: testIds.DEFAULT_INSTALLATION_BUTTON,
              },
              {
                label: 'Locate other installation',
                value: 'other',
                testID: testIds.OTHER_INSTALLATION_BUTTON,
              },
            ]}
            value={installation}
            onChange={(installation: Installation) => {
              setInstallation(installation);
              if (
                installation === 'default' &&
                typeof defaultInstallationPath === 'string'
              ) {
                onChange(defaultInstallationPath);
              } else {
                onChange(otherInstallationPath);
              }
            }}
          />
        </div>
      )}
      {version === '8-player' ||
      (version === '4-player' && installation === 'other') ||
      (version === '4-player' && defaultInstallationPath === null) ? (
        <div>
          <p>Locate the TowerFall installation you wish to patch:</p>
          <button
            onClick={async () => {
              const file = await window.api.browseFiles();
              onChange(file);
              setOtherInstallationPath(file);
            }}
            data-testid={testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON}
          >
            Browse
          </button>
        </div>
      ) : null}
    </div>
  );
};

export default FileBrowser;
