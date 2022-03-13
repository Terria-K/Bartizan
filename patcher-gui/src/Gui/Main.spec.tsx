import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom/extend-expect';
import userEvent from '@testing-library/user-event';
import Main from './Main';
import { ButtonStatuses } from './ActionButtons';
import testIds from './test-helpers/testIds';
import { selectOption } from './test-helpers';

window.api = {
  browseFiles: jest.fn(),
  checkForDefaultInstallation: jest.fn(),
  checkPatchability: jest.fn(),
};

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

  it('shows patch button if default installation found', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);

    withCheckPatchabilityReturning({
      canPatch: true,
      canUnpatch: true,
    });

    render(<Main />);

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /4-player/i);

    const patchButton = await screen.findByText('Patch');
    expect(patchButton).toBeTruthy();
  });

  it('shows browse button if 4-player selected and no default installation found', async () => {
    withCheckForDefaultInstallationReturning('');

    render(<Main />);

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /4-player/i);

    expect(
      await screen.findByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    ).toBeTruthy();
  });

  it('shows default path if 4-player selected and default installation is found', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: true });

    render(<Main />);

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /4-player/i);

    expect((await screen.findAllByText(pathToGame)).length).toEqual(2);
  });

  it('shows enabled patch and unpatch buttons if patchability check returns true values', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: true });

    render(<Main />);

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /4-player/i);

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

    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /4-player/i);

    const patchButton = await screen.findByText('Patch');
    const unpatchButton = screen.getByText('Unpatch');

    expect(patchButton).toHaveAttribute('disabled');
    expect(unpatchButton).toHaveAttribute('disabled');
  });

  it('shows custom path when user switches back to 4-player having selecting one earlier and deselecting 4-player', async () => {
    const pathToGame = `/Path/To/Game`;
    withCheckForDefaultInstallationReturning(pathToGame);
    withCheckPatchabilityReturning({ canPatch: true, canUnpatch: true });

    render(<Main />);

    // Select 4-player
    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /4-player/i);

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

    // De-select 4-player
    selectOption(
      screen.getByTestId(testIds.VERSION_SELECT_INPUT),
      /Select One/i
    );

    // Select path appears on the screen
    await expect(screen.findByText(otherPathToGame)).rejects.toThrow();

    // Re-select 4-player
    selectOption(screen.getByTestId(testIds.VERSION_SELECT_INPUT), /4-player/i);

    // Select path reappears on the screen
    expect(await screen.findByText(otherPathToGame)).toBeTruthy();
  });
});
