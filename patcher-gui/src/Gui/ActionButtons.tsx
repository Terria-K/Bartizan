import React, { useEffect, useState } from 'react';
import Button from './general/Button';
import FilePath from './general/FilePath';
import { ButtonStatuses } from '../types';

type ActionButtonsProps = {
  towerfallPath: string;
  towerfallVersion: string;
  onPatchSuccess: () => void;
  onPatchFail: () => void;
  onUnpatchSuccess: () => void;
  onUnpatchFail: () => void;
};

const ActionButtons: React.FC<ActionButtonsProps> = ({
  towerfallPath,
  towerfallVersion,
  onPatchSuccess,
  onPatchFail,
  onUnpatchSuccess,
  onUnpatchFail,
}) => {
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
        onClick={() =>
          window.api
            .patch(towerfallPath, towerfallVersion)
            .then((success) => (success ? onPatchSuccess() : onPatchFail()))
        }
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
