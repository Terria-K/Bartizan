import React from 'react';
import {
  FormControl,
  FormControlLabel,
  Radio,
  RadioGroup,
} from '@mui/material';

type RadioButtonsProps<T> = {
  options: { label: string; value: T; testID?: string }[];
  value: T;
  onChange: (value: T) => void;
};

const RadioButtons = function <T extends string | number>({
  options,
  value,
  onChange,
}: RadioButtonsProps<T>) {
  return (
    <FormControl>
      <RadioGroup
        value={value}
        onChange={(event, value) => onChange(value as T)}
      >
        {options.map((option) => (
          <FormControlLabel
            key={option.value}
            value={option.value}
            control={<Radio data-testid={option.testID} />}
            label={option.label}
          />
        ))}
      </RadioGroup>
    </FormControl>
  );
};

export default RadioButtons;
