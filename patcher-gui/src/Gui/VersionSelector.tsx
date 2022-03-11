import React from 'react';
import testIds from './test-helpers/testIds';

export type Version = '4-player' | '8-player';

type VersionSelectorProps = {
  onChange: (version: Version) => void;
};

const VersionSelector: React.FC<VersionSelectorProps> = ({ onChange }) => {
  return (
    <div>
      <p>Select the version of TowerFall you wish to patch:</p>
      <select
        name="tfVersion"
        data-testid={testIds.VERSION_SELECT_INPUT}
        onChange={(event) => {
          onChange(event.target.value as Version);
        }}
      >
        <option value={''}>(Select One)</option>
        <option value="4-player">4-Player (version 1.3.3.3)</option>
        <option value="8-player">8-Player (version 1.3.2.0)</option>
      </select>
    </div>
  );
};

export default VersionSelector;
