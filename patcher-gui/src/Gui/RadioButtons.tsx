import React from 'react';

type RadioButtonsProps<T> = {
  options: { label: string; value: T }[];
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
        <label>
          {option.label}
          <input
            type="radio"
            value={option.value}
            checked={value === option.value}
            onChange={(event) => onChange(event.target.value as T)}
          />
        </label>
      ))}
    </div>
  );
};

export default RadioButtons;
