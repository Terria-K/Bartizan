import React from 'react';
import type { Version } from './VersionSelector';

type FileBrowserProps = {
  version: Version;
};

const FileBrowser: React.FC<FileBrowserProps> = ({ version }) => {
  if (!version) {
    return null;
  }

  return (
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
  );
};

export default FileBrowser;
