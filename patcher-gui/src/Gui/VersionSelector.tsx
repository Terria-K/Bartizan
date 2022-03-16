import React from 'react';
import { FormControl, Select, MenuItem, InputLabel } from '@mui/material';
import testIds from './test-helpers/testIds';
import { Version } from '../types';

type VersionSelectorProps = {
  towerfallVersion: Version;
  onChange: (version: Version) => void;
};

const VersionSelector: React.FC<VersionSelectorProps> = ({
  onChange,
  towerfallVersion,
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
          value={towerfallVersion || ''}
        >
          <MenuItem value="">(Select One)</MenuItem>
          <MenuItem value="4-player-steam">
            Steam 4-Player (version 1.3.3.3)
          </MenuItem>
          <MenuItem value="4-player-itch">
            Itch.io 4-Player (version 1.3.3.1)
          </MenuItem>
          <MenuItem value="8-player">8-Player (version 1.3.2.0)</MenuItem>
        </Select>
      </FormControl>
    </div>
  );
};

export default VersionSelector;
