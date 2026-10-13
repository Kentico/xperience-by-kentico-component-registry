import React from 'react';
import { ComponentIcon } from './component-icon';
import { TableCell, TableHead } from './table';

const iconColumnClass = 'w-16 px-2 text-center';

interface IconTableCellProps {
  readonly iconClass?: string;
}

const IconTableCell = ({ iconClass }: IconTableCellProps) => (
  <TableCell className={iconColumnClass}>
    {iconClass ? (
      <ComponentIcon iconClass={iconClass} />
    ) : (
      <span className="xp-muted-dash">—</span>
    )}
  </TableCell>
);

const IconTableHead = () => (
  <TableHead className={`${iconColumnClass} font-semibold`}>Icon</TableHead>
);

export { IconTableCell, IconTableHead };
