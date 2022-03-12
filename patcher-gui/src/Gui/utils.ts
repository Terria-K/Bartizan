import { useEffect, useRef } from 'react';

export function usePrevious(value: unknown) {
  const ref = useRef(value);
  useEffect(() => {
    ref.current = value;
  }, [ref, value]);
  return ref.current;
}
