import React from 'react';

import { cn } from '../../lib/utils';

type CalloutType = 'info' | 'warning';

interface CalloutProps extends React.HTMLAttributes<HTMLDivElement> {
  type?: CalloutType;
}

export const Callout = React.forwardRef<HTMLDivElement, CalloutProps>(
  ({ className, type = 'info', ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        'xp-callout',
        type === 'warning' && 'xp-callout-warning',
        type === 'info' && 'xp-callout-info',
        className,
      )}
      {...props}
    />
  ),
);

Callout.displayName = 'Callout';
