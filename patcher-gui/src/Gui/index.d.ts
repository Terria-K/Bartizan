import { ButtonStatuses } from './ActionButtons';

export interface API {
  checkForDefaultInstallation: () => Promise<string>;
  browseFiles: () => Promise<string>;
  checkPatchability: (towerfallPath: string) => Promise<ButtonStatuses>;
  patch: (towerfallPath: string, towerfallVersion: string) => Promise<boolean>;
}

declare global {
  interface Window {
    api: API;
  }
}
