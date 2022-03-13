import React from 'react';
import { FormControl, Select, MenuItem, InputLabel } from '@mui/material';
import testIds from './test-helpers/testIds';

export type Version = '4-player' | '8-player';

type VersionSelectorProps = {
  version: Version;
  onChange: (version: Version) => void;
};

const VersionSelector: React.FC<VersionSelectorProps> = ({
  onChange,
  version,
}) => {
  return (
    <div>
      <h3>Select the version of TowerFall you wish to patch:</h3>
      <FormControl sx={{ minWidth: 200 }}>
        <InputLabel id="version-selector">Version</InputLabel>
        <Select
          labelId="version-selector"
          data-testid={testIds.VERSION_SELECT_INPUT}
          onChange={(event) => {
            onChange(event.target.value as Version);
          }}
          label="Version"
          value={version || ''}
        >
          <MenuItem value="">(Select One)</MenuItem>
          <MenuItem value="4-player">4-Player (version 1.3.3.3)</MenuItem>
          <MenuItem value="8-player">8-Player (version 1.3.2.0)</MenuItem>
        </Select>
      </FormControl>
    </div>
  );
};

export default VersionSelector;
