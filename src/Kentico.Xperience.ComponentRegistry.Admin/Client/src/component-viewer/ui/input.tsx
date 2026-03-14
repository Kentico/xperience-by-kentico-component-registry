import React from 'react';

import { cn } from '../../lib/utils';

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  wrapperClassName?: string;
}

export const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, wrapperClassName, ...props }, ref) => (
    <div className={cn('xp-input-wrapper', wrapperClassName)}>
      <input ref={ref} className={cn('xp-input', className)} {...props} />
    </div>
  ),
);

Input.displayName = 'Input';
