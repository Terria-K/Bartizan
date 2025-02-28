import { useEffect, useRef } from 'react';

export function usePrevious<T>(value: T) {
  const ref = useRef(value);
  useEffect(() => {
    ref.current = value;
  }, [ref, value]);
  return ref.current;
}
