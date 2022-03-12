import React from 'react';

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
    <div>
      {options.map((option) => (
        <label key={option.value}>
          {option.label}
          <input
            type="radio"
            value={option.value}
            checked={value === option.value}
            onChange={(event) => onChange(event.target.value as T)}
            data-testid={option.testID}
          />
        </label>
      ))}
    </div>
  );
};

export default RadioButtons;
