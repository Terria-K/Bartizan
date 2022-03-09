import React, { useEffect, useState } from 'react';
import type { Version } from './VersionSelector';
import RadioButtons from './RadioButtons';

type FileBrowserProps = {
  version: Version;
};

type Installation = 'default' | 'other';

const FileBrowser: React.FC<FileBrowserProps> = ({ version }) => {
  const [defaultInstallationPath, setDefaultInstallationPath] = useState<
    string | boolean
  >(null);
  const [installation, setInstallation] = useState<Installation>('default');

  useEffect(() => {
    if (version === '4-player') {
      window.api
        .checkForDefaultInstallation()
        .then((path: string | boolean) => {
          setDefaultInstallationPath(path);
        });
    }
  }, [version]);

  if (!version) {
    return null;
  }

  return (
    <div>
      {version === '4-player' && !!defaultInstallationPath && (
        <div>
          <p>Found TowerFall installed at "{defaultInstallationPath}"</p>
          <RadioButtons<Installation>
            options={[
              { label: 'Select this installation', value: 'default' },
              { label: 'Find other installation', value: 'other' },
            ]}
            value={installation}
            onChange={setInstallation}
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
              console.log(file);
            }}
          >
            Browse
          </button>
        </div>
      ) : null}
    </div>
  );
};

export default FileBrowser;
