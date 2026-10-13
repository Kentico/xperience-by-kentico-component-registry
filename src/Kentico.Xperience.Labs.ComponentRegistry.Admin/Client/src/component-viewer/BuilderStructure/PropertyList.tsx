import React from 'react';
import { BuilderPropertyDto } from './types';

interface PropertyListProps {
  readonly title: string;
  readonly properties: readonly BuilderPropertyDto[];
}

export const PropertyList = ({ title, properties }: PropertyListProps) => (
  <div>
    <div className="mb-1 text-xs font-semibold uppercase tracking-wide xp-text-muted">
      {title}
    </div>
    <dl className="grid grid-cols-[minmax(8rem,max-content)_minmax(0,1fr)] gap-x-4 gap-y-1 rounded-md xp-panel-subtle px-3 py-2 text-sm">
      {properties.map((property) => (
        <React.Fragment key={property.name}>
          <dt className="font-mono text-xs leading-5 xp-text-muted">
            {property.name}
          </dt>
          <dd className="m-0 min-w-0" title={property.value}>
            {property.value === 'null' || property.value === '' ? (
              <span className="xp-muted-dash">—</span>
            ) : (
              <span className="line-clamp-2 break-all font-mono text-xs leading-5 xp-text-default">
                {property.value}
              </span>
            )}
          </dd>
        </React.Fragment>
      ))}
    </dl>
  </div>
);
