import React, { useEffect, useState } from 'react';
import Button from './general/Button';

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
      <p>
        <strong>Selected Path:</strong> {towerfallPath}
      </p>
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
