import { ButtonStatuses } from './ActionButtons';
import { Version } from './VersionSelector';

export interface API {
  checkForDefaultInstallation: (towerfallVersion: Version) => Promise<string>;
  browseFiles: () => Promise<string>;
  checkPatchability: (towerfallPath: string) => Promise<ButtonStatuses>;
  patch: (towerfallPath: string, towerfallVersion: Version) => Promise<boolean>;
  unpatch: (towerfallPath: string, towerfallVersion: Version) => Promise<boolean>;
}

declare global {
  interface Window {
    api: API;
  }
}
