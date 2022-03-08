export interface API {
  checkForDefaultInstallation: () => Promise<string>;
  browseFiles: () => Promise<string>;
}

declare global {
  interface Window {
    api: API;
  }
}
