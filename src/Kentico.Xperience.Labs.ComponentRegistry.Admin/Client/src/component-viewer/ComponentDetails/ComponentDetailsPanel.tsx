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
  const lastModified = data.lastModified
    ? new Date(data.lastModified)
    : undefined;

  if (isPageUsageData(data)) {
    return (
      <div className="p-4 bg-slate-50 rounded-lg border border-slate-200">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          {/* Statistics Column */}
          <div>
            <UsageStatistics
              totalPages={data.totalPagesUsing}
              totalVariants={data.totalVariants}
              lastModified={lastModified}
              label="Pages Using Component"
            />
          </div>

          {/* Pages List Column */}
          <div className="md:col-span-3">
            <PageList
              pages={data.pages}
              inspectedComponentIdentifier={data.componentIdentifier}
              inspectedComponentType={data.componentType}
              inspectedComponentTypeName={inspectedComponentTypeName}
            />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="p-4 bg-slate-50 rounded-lg border border-slate-200">
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        {/* Statistics Column */}
        <div>
          <UsageStatistics
            totalPages={data.totalEmailConfigurationsUsing}
            totalVariants={data.totalVariants}
            lastModified={lastModified}
            label="Configurations Using Component"
          />
        </div>

        {/* Email Configurations List Column */}
        <div className="md:col-span-3">
          <EmailConfigurationList
            emailConfigurations={data.emailConfigurations}
            inspectedComponentIdentifier={data.componentIdentifier}
            inspectedComponentType={data.componentType}
            inspectedComponentTypeName={inspectedComponentTypeName}
          />
        </div>
      </div>
    </div>
  );
};
