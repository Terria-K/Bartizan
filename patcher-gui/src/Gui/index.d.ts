import ButtonStatuses from './ActionButtons';

export interface API {
  checkForDefaultInstallation: () => Promise<string>;
  browseFiles: () => Promise<string>;
  checkPatchability: (towerfallPath: string) => Promise<ButtonStatuses>;
}

declare global {
  interface Window {
    api: API;
  }
}
