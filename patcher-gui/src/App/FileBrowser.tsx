import React from "react";

const FileBrowser: React.FC = () => {
  return (
    <div>
      <p>Locate the TowerFall installation you wish to patch:</p>
      <input type="file" onChange={(event) => console.log(event)} />
    </div>
  );
};

export default FileBrowser;
