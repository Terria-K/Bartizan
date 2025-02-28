import { screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';

// Select an option in a MUI Select component
export function selectOption(
  element: HTMLElement,
  optionText: RegExp | string
) {
  userEvent.click(within(element).getByRole('button'));
  const listbox = screen.getByRole('listbox');
  userEvent.click(within(listbox).getByText(optionText));
}
