import React from "react";
import { version } from "../../package.json";
import VersionSelector, { Version } from "./VersionSelector";
import FileBrowser from "./FileBrowser";

type MainState = {
  version: Version;
};

const Main: React.FC = () => {
  return (
    <div>
      <h1>Bartizan (Plus) Patcher</h1>
      <h4>Version: {version}</h4>
      <VersionSelector onChange={(version) => console.log(version)} />
      <FileBrowser />
    </div>
  );
};

export default Main;
