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
  const [loading, setLoading] = useState<boolean>(false);
  const [buttonStatuses, setButtonStatuses] = useState<ButtonStatuses>({
    canPatch: false,
    canUnpatch: false,
  });

  useEffect(() => {
    if (towerfallPath) {
      setLoading(true);
      window.api.checkPatchability(towerfallPath).then((statuses) => {
        setLoading(false);
        setButtonStatuses(statuses);
      });
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
        disabled={!buttonStatuses.canPatch || loading}
        onClick={() => {
          setLoading(true);
          window.api
            .patch(towerfallPath, towerfallVersion)
            .then((success) => {
              setLoading(false);
              success ? onPatchSuccess() : onPatchFail();
            })
            .catch(() => {
              setLoading(false);
              onPatchFail();
            });
        }}
      >
        Patch
      </Button>
      &nbsp;
      <Button
        disabled={!buttonStatuses.canUnpatch || loading}
        onClick={() => console.log('unpatch')}
      >
        Unpatch
      </Button>
    </div>
  );
};

export default ActionButtons;
