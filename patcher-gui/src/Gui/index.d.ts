export interface API {
  checkForDefaultInstallation: () => Promise<void>;
  browseFiles: () => Promise<string>;
}

declare global {
  interface Window {
    api: API;
  }
}
