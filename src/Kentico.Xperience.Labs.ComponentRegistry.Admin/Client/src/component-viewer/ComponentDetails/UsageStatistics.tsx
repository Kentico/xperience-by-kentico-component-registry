import React from 'react';
import { UsageStats } from './UsageStats';

interface UsageStatisticsProps {
  totalPages: number;
  totalVariants: number;
  label?: string;
}

export const UsageStatistics: React.FC<UsageStatisticsProps> = ({
  totalPages,
  totalVariants,
  label = 'Pages Using Component',
}) => {
  return (
    <UsageStats
      stats={[
        { label, value: totalPages },
        { label: 'Language Variants', value: totalVariants },
      ]}
    />
  );
};
