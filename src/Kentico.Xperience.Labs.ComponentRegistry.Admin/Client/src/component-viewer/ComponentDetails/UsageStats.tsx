import React from 'react';

export interface UsageStat {
  label: string;
  value: React.ReactNode;
}

export const UsageStats: React.FC<{ stats: UsageStat[] }> = ({ stats }) => (
  <div className="flex flex-wrap items-center gap-2 text-xs">
    {stats.map((stat) => (
      <span
        key={stat.label}
        className="inline-flex items-baseline gap-1.5 px-2 py-1 bg-[#f5f5f5] rounded border border-[#dfdfdf]"
      >
        <span className="xp-text-muted font-medium">{stat.label}</span>
        <span className="font-bold xp-text-default">{stat.value}</span>
      </span>
    ))}
  </div>
);
