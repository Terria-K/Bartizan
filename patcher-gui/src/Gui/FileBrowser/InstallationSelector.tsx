import React from 'react';
import { Version } from '../VersionSelector';
import RadioButtons from '../general/RadioButtons';
import FilePath from '../general/FilePath';
import testIds from '../test-helpers/testIds';

type Installation = 'default' | 'other';

type InstallationSelectorProps = {
  version: Version;
  defaultInstallationPath: string;
  installation: Installation;
  onChange: (value: Installation) => void;
};

const InstallationSelector: React.FC<InstallationSelectorProps> = ({
  version,
  defaultInstallationPath,
  installation,
  onChange,
}) => {
  if (version !== '4-player' || !defaultInstallationPath) {
    return null;
  }

  return (
    <div>
      <h3>Found TowerFall installed at:</h3>
      <FilePath path={defaultInstallationPath} />
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
          onChange(installation);
        }}
      />
    </div>
  );
};

export default InstallationSelector;
