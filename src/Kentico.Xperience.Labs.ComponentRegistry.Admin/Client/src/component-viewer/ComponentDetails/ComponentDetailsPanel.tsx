import React from 'react';
import { PageList } from './PageList';
import { EmailConfigurationList } from './EmailConfigurationList';
import { UsageStatistics } from './UsageStatistics';
import {
  ComponentUsageDetailDto,
  EmailConfigurationUsageDetailDto,
} from './types';

interface ComponentDetailsPanelProps {
  data: ComponentUsageDetailDto | EmailConfigurationUsageDetailDto;
  inspectedComponentTypeName?: string;
}

const isPageUsageData = (
  data: ComponentUsageDetailDto | EmailConfigurationUsageDetailDto,
): data is ComponentUsageDetailDto => {
  return 'pages' in data;
};

export const ComponentDetailsPanel: React.FC<ComponentDetailsPanelProps> = ({
  data,
  inspectedComponentTypeName,
}) => {
  if (isPageUsageData(data)) {
    return (
      <div className="p-4 bg-slate-50 rounded-lg border border-slate-200 space-y-3">
        <UsageStatistics
          totalPages={data.totalPagesUsing}
          totalVariants={data.totalVariants}
          label="Pages Using Component"
        />
        <PageList
          pages={data.pages}
          inspectedComponentIdentifier={data.componentIdentifier}
          inspectedComponentType={data.componentType}
          inspectedComponentTypeName={inspectedComponentTypeName}
        />
      </div>
    );
  }

  return (
    <div className="p-4 bg-slate-50 rounded-lg border border-slate-200 space-y-3">
      <UsageStatistics
        totalPages={data.totalEmailConfigurationsUsing}
        totalVariants={data.totalVariants}
        label="Configurations Using Component"
      />
      <EmailConfigurationList
        emailConfigurations={data.emailConfigurations}
        inspectedComponentIdentifier={data.componentIdentifier}
        inspectedComponentType={data.componentType}
        inspectedComponentTypeName={inspectedComponentTypeName}
      />
    </div>
  );
};
