import React from 'react';

type FilePathProps = {
  path: string;
};

const nonBreakingSpace = '\u00a0';

const FilePath: React.FC<FilePathProps> = ({ path }) => {
  return (
    <p
      style={{
        overflowWrap: 'break-word',
        lineHeight: 1.5,
      }}
    >
      {path.replace(/\s/m, nonBreakingSpace)}
    </p>
  );
};

export default FilePath;
