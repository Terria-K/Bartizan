import React from 'react';
import { Snackbar, Alert as MuiAlert } from '@mui/material';
import type { AlertColor } from '@mui/material';

type AlertType = {
  message: string;
  color: AlertColor;
};

type AlertProps = {
  alert: AlertType;
  onClose: () => void;
};

const Alert: React.FC<AlertProps> = ({ alert, onClose }) => {
  const open = !!alert?.message;
  return (
    <Snackbar
      open={open}
      autoHideDuration={6000}
      onClose={onClose}
      anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
    >
      <MuiAlert
        onClose={onClose}
        severity={alert?.color}
        sx={{ width: '100%' }}
      >
        {alert?.message}
      </MuiAlert>
    </Snackbar>
  );
};

export default Alert;
