import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import userEvent from '@testing-library/user-event';
import Main from './Main';
import { ButtonStatuses } from '../types';
import testIds from './test-helpers/testIds';
import { selectOption } from './test-helpers';

window.api = {
  browseFiles: jest.fn(),
  checkForDefaultInstallation: jest.fn(),
  checkPatchability: jest.fn(),
  patch: jest.fn(),
};

const versionNone = /Select One/i;
const versionSteam = /Steam 4-player/i;
const versionItch = /Itch 4-player/i;

describe('Main', () => {
  const withCheckForDefaultInstallationReturning = (path: string) => {
    (window.api.checkForDefaultInstallation as jest.Mock).mockImplementation(
      () => Promise.resolve(path)
    );
  };

  const withBrowseFilesReturning = (path: string) => {
    (window.api.browseFiles as jest.Mock).mockImplementation(() =>
      Promise.resolve(path)
    );
  };

  const withCheckPatchabilityReturning = (patchability: ButtonStatuses) => {
    (window.api.checkPatchability as jest.Mock).mockImplementation(() =>
      Promise.resolve(patchability)
    );
  };

  const withPatchReturning = (result: boolean) => {
    (window.api.patch as jest.Mock).mockReturnValue(Promise.resolve(result));
  };

  it('does not check for default installation if 8-player selected and shows browse button', async () => {
    render(<Main />);

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /8-player/i);

    expect(window.api.checkForDefaultInstallation).not.toBeCalled();

    expect(
      screen.getByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();
  });

  it('shows patch button after valid path selected', async () => {
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: false });
    const selectedPath = `/Other/Path`;
    withBrowseFilesReturning(selectedPath);

    render(<Main />);

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /8-player/i);

    userEvent.click(
      screen.getByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    );

    expect(await screen.findByText('Patch'));
  });

  it('shows patch button if default installation found and default installation selected', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);

    withCheckPatchabilityReturning({
      canPatch: true,
      canUnpatch: true,
    });

    render(<Main />);

    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    userEvent.click(
      await screen.findByTestId(testIds.DEFAULT_INSTALLATION_BUTTON)
    );

    expect(await screen.findByText('Patch')).toBeTruthy();
  });

  it('shows browse button if 4-player-steam selected and no default installation found', async () => {
    withCheckForDefaultInstallationReturning('');

    render(<Main />);

    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    expect(
      await screen.findByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();
  });

  it('shows default path if 4-player-steam selected and default installation is found', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: true });

    render(<Main />);

    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    expect(await screen.findByText(pathToGame)).toBeTruthy();
  });

  it('shows enabled patch and unpatch buttons if patchability check returns true values', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: true });

    render(<Main />);

    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    userEvent.click(
      await screen.findByTestId(testIds.DEFAULT_INSTALLATION_BUTTON)
    );

    const patchButton = await screen.findByText('Patch');
    const unpatchButton = screen.getByText('Unpatch');

    expect(patchButton).not.toHaveAttribute('disabled');
    expect(unpatchButton).not.toHaveAttribute('disabled');
  });

  it('shows disabled patch and unpatch buttons if patchability check returns false values', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: false, canUnpatch: false });

    render(<Main />);

    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    userEvent.click(
      await screen.findByTestId(testIds.DEFAULT_INSTALLATION_BUTTON)
    );

    const patchButton = await screen.findByText('Patch');
    const unpatchButton = screen.getByText('Unpatch');

    expect(patchButton).toHaveAttribute('disabled');
    expect(unpatchButton).toHaveAttribute('disabled');
  });

  it('shows custom path when user switches back to 4-player-steam having selecting one earlier and deselecting 4-player-steam', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: true });

    render(<Main />);

    // Select 4-player-steam
    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    // Select other installation
    userEvent.click(
      await screen.findByTestId(testIds.OTHER_INSTALLATION_BUTTON)
    );

    // Browse and select path
    const otherPathToGame = `/Other/Path`;
    (window.api.browseFiles as jest.Mock).mockImplementation(() =>
      Promise.resolve(otherPathToGame)
    );
    userEvent.click(
      screen.getByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    );

    // Select path appears on the screen
    expect(await screen.findByText(otherPathToGame)).toBeTruthy();

    // De-select 4-player-steam
    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), versionNone);

    // Select path appears on the screen
    await expect(screen.findByText(otherPathToGame)).rejects.toThrow();

    // Re-select 4-player-steam
    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    // Select path reappears on the screen
    expect(await screen.findByText(otherPathToGame)).toBeTruthy();
  });

  it('shows file browse button if Itch version selected and no path is returned', async () => {
    withCheckForDefaultInstallationReturning('');

    render(<Main />);

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), versionItch);

    expect(
      await screen.findByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();

    // This test was erroneously passing because the button was shown for an instant.
    // Check for the button again to make sure this isn't happening
    expect(
      await screen.findByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();
  });

  it('shows file browse button if Steam version selected and no path is returned', async () => {
    render(<Main />);

    withCheckForDefaultInstallationReturning('');
    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    expect(
      await screen.findByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();

    // This test was erroneously passing because the button was shown for an instant.
    // Check for the button again to make sure this isn't happening
    expect(
      await screen.findByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();
  });

  it('shows browse button if Steam with default found selected but then Itch selected with no default', async () => {
    render(<Main />);

    withCheckForDefaultInstallationReturning('/Some/Path');
    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    withCheckForDefaultInstallationReturning('');
    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), versionItch);

    expect(
      await screen.findByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();
  });

  it('shows installation selector if Itch with no default selected but then Steam selected with default', async () => {
    render(<Main />);

    withCheckForDefaultInstallationReturning('');
    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), versionItch);

    withCheckForDefaultInstallationReturning('/Some/Path');
    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    expect(
      await screen.findByTestId(testIds.VERSION_SELECT_INPUT)
    ).toBeTruthy();
  });

  it('shows Itch path if Steam with default selected and then Itch with default selected', async () => {
    const itchPath = '/Itch/Path';
    const steamPath = '/Steam/Path';

    render(<Main />);

    withCheckForDefaultInstallationReturning(steamPath);
    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    withCheckForDefaultInstallationReturning(itchPath);
    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), versionItch);

    expect(await screen.findByText(itchPath)).toBeTruthy();
  });

  it('shows success message if api.patch returns true', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: false });
    withPatchReturning(true);

    render(<Main />);

    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    userEvent.click(
      await screen.findByTestId(testIds.DEFAULT_INSTALLATION_BUTTON)
    );

    userEvent.click(await screen.findByText('Patch'));

    expect(await screen.findByText(/success/i)).toBeTruthy();
  });

  it('shows failure message if api.patch returns false', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: false });
    withPatchReturning(false);

    render(<Main />);

    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      versionSteam
    );

    userEvent.click(
      await screen.findByTestId(testIds.DEFAULT_INSTALLATION_BUTTON)
    );

    userEvent.click(await screen.findByText('Patch'));

    expect(await screen.findByText(/fail/i)).toBeTruthy();
  });
});
