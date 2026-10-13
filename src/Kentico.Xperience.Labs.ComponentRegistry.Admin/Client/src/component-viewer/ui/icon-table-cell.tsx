import { Icon, type IconName } from '@kentico/xperience-admin-components';
import React from 'react';
import { TableCell, TableHead } from './table';

const iconColumnClass = 'w-16 px-2 text-center';

// Registration attributes use legacy "icon-*" names; admin icon set uses "xp-*"
const toIconName = (iconClass: string): IconName =>
  iconClass.replace(/^icon-/, 'xp-') as IconName;

interface IconTableCellProps {
  readonly iconClass?: string;
}

const IconTableCell = ({ iconClass }: IconTableCellProps) => (
  <TableCell className={iconColumnClass}>
    {iconClass ? (
      <span
        className="inline-flex justify-center"
        title={iconClass}
        aria-label={iconClass}
      >
        <Icon name={toIconName(iconClass)} fallbackIconName="xp-question-circle" />
      </span>
    ) : (
      <span className="xp-muted-dash">—</span>
    )}
  </TableCell>
);

const IconTableHead = () => (
  <TableHead className={`${iconColumnClass} font-semibold`}>Icon</TableHead>
);

export { IconTableCell, IconTableHead };
