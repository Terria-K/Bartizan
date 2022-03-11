import React from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import Main from './Main';
import testIds from './test-helpers/testIds';

describe('Main', () => {
  it('displays custom path when user switches back to 4-player having selecting one earlier and deselecting 4-player', () => {
    render(<Main />);

    userEvent.selectOptions(screen.getByTestId(testIds.VERSION_SELECT_INPUT), [
      '4-player',
    ]);
  });
});
