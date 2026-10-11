import React from 'react';

import { cn } from '../../lib/utils';

type HeadlineSize = 'S' | 'M' | 'L';

interface HeadlineProps extends React.HTMLAttributes<HTMLDivElement> {
  size?: HeadlineSize;
}

export const Headline = React.forwardRef<HTMLDivElement, HeadlineProps>(
  ({ className, size = 'L', ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        'xp-headline',
        size === 'S' && 'xp-headline-s',
        size === 'M' && 'xp-headline-m',
        size === 'L' && 'xp-headline-l',
        className,
      )}
      {...props}
    />
  ),
);

Headline.displayName = 'Headline';
