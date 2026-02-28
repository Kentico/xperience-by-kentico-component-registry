import React, { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { LanguageVariantList } from './LanguageVariantList';
import { EmailConfigurationUsageDto } from './types';

interface EmailConfigurationListItemProps {
  emailConfiguration: EmailConfigurationUsageDto;
  inspectedComponentIdentifier: string;
  inspectedComponentType: string;
  inspectedComponentTypeName?: string;
}

export const EmailConfigurationListItem: React.FC<
  EmailConfigurationListItemProps
> = ({
  emailConfiguration,
  inspectedComponentIdentifier,
  inspectedComponentType,
  inspectedComponentTypeName,
}) => {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="border border-slate-300 rounded-lg overflow-hidden bg-white">
      <button
        onClick={() => setExpanded(!expanded)}
        className="w-full px-4 py-3 flex items-center justify-between hover:bg-slate-50 transition-colors text-left"
      >
        <div className="flex-1">
          <p className="font-semibold text-slate-900">
            {emailConfiguration.configurationName}
          </p>
          <p className="text-sm text-slate-600">
            {emailConfiguration.configurationPurpose}
          </p>
          <p className="text-xs text-slate-500">
            {emailConfiguration.channelDisplayName}
          </p>
        </div>
        <ChevronDown
          size={20}
          className={`text-slate-600 transition-transform flex-shrink-0 ml-2 ${
            expanded ? '' : '-rotate-90'
          }`}
        />
      </button>

      {expanded && (
        <div className="px-4 py-3 bg-slate-50 border-t border-slate-300">
          <LanguageVariantList
            variants={emailConfiguration.variants}
            inspectedComponentIdentifier={inspectedComponentIdentifier}
            inspectedComponentType={inspectedComponentType}
            inspectedComponentTypeName={inspectedComponentTypeName}
          />
        </div>
      )}
    </div>
  );
};
