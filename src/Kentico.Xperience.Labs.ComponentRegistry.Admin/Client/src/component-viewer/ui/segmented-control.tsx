import React from 'react';
import { cn } from '../../lib/utils';

export interface SegmentedControlOption<T extends string> {
  readonly value: T;
  readonly label: string;
}

type ValueChangeHandler<T> = (value: T) => void;

interface SegmentedControlProps<T extends string> {
  readonly options: readonly SegmentedControlOption<T>[];
  readonly value: T;
  readonly onChange: ValueChangeHandler<T>;
  readonly ariaLabel: string;
  readonly className?: string;
}

export const SegmentedControl = <T extends string>({
  options,
  value,
  onChange,
  ariaLabel,
  className,
}: SegmentedControlProps<T>) => (
  <div
    role="group"
    aria-label={ariaLabel}
    className={cn('xp-segmented', className)}
  >
    {options.map((option) => (
      <button
        key={option.value}
        type="button"
        aria-pressed={option.value === value}
        data-state={option.value === value ? 'active' : 'inactive'}
        onClick={() => onChange(option.value)}
        className="xp-segmented-option"
      >
        {option.label}
      </button>
    ))}
  </div>
);
