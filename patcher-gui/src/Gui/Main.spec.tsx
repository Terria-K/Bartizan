import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import Main from './Main';
import testIds from './test-helpers/testIds';

window.api = {
  browseFiles: jest.fn(),
  checkForDefaultInstallation: jest.fn(),
  checkPatchability: jest.fn(),
};

describe('Main', () => {
  it('displays custom path when user switches back to 4-player having selecting one earlier and deselecting 4-player', async () => {
    const pathToGame = `/Path/To/Game`;
    (window.api.checkForDefaultInstallation as jest.Mock).mockImplementation(
      () => Promise.resolve(pathToGame)
    );
    (window.api.checkPatchability as jest.Mock).mockImplementation(() =>
      Promise.resolve({
        canPatch: true,
        canUnpatch: true,
      })
    );

    render(<Main />);

    // Select 4-player
    userEvent.selectOptions(screen.getByTestId(testIds.VERSION_SELECT_INPUT), [
      '4-player',
    ]);

    // Select other installation
    userEvent.click(
      await screen.findByTestId(testIds.OTHER_INSTALLATION_BUTTON)
    );

    // Browse and select path
    const otherPathToGame = `/Other/Patch`;
    (window.api.browseFiles as jest.Mock).mockImplementation(() =>
      Promise.resolve(otherPathToGame)
    );
    userEvent.click(
      screen.getByTestId(testIds.BROWSE_FOR_OTHER_INSTALLATION_BUTTON)
    );

    // Select path appears on the screen
    expect(await screen.findByText(otherPathToGame)).toBeTruthy();

    // De-select 4-player
    userEvent.selectOptions(screen.getByTestId(testIds.VERSION_SELECT_INPUT), [
      '',
    ]);

    // Select path appears on the screen
    await expect(screen.findByText(otherPathToGame)).rejects.toThrow();

    // Re-select 4-player
    userEvent.selectOptions(screen.getByTestId(testIds.VERSION_SELECT_INPUT), [
      '4-player',
    ]);

    // Select path reappears on the screen
    expect(await screen.findByText(otherPathToGame)).toBeTruthy();
  });
});
