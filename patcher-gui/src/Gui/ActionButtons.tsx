import React, { useEffect, useState } from 'react';
import Button from './general/Button';
import FilePath from './general/FilePath';

export type ButtonStatuses = {
  canPatch: boolean;
  canUnpatch: boolean;
};

type ActionButtonsProps = {
  towerfallPath: string;
};

const ActionButtons: React.FC<ActionButtonsProps> = ({ towerfallPath }) => {
  const [buttonStatuses, setButtonStatuses] = useState<ButtonStatuses>({
    canPatch: false,
    canUnpatch: false,
  });

  useEffect(() => {
    if (towerfallPath) {
      window.api.checkPatchability(towerfallPath).then(setButtonStatuses);
    } else {
      setButtonStatuses({ canPatch: false, canUnpatch: false });
    }
  }, [towerfallPath]);

  if (!towerfallPath) {
    return null;
  }

  return (
    <div>
      <h3>Selected Path:</h3>
      <FilePath path={towerfallPath} />
      <Button
        disabled={!buttonStatuses.canPatch}
        onClick={() => console.log('patch')}
      >
        Patch
      </Button>
      &nbsp;
      <Button
        disabled={!buttonStatuses.canUnpatch}
        onClick={() => console.log('unpatch')}
      >
        Unpatch
      </Button>
    </div>
  );
};

export default ActionButtons;
