import React from 'react';
import { Button as MuiButton } from '@mui/material';

type ButtonProps = {
  onClick: () => void;
  children: React.ReactNode;
  disabled?: boolean;
};

const Button: React.FC<ButtonProps> = ({
  onClick,
  children,
  disabled,
  ...rest
}) => {
  return (
    <MuiButton
      variant="outlined"
      onClick={onClick}
      disabled={disabled}
      {...rest}
    >
      {children}
    </MuiButton>
  );
};

export default Button;
