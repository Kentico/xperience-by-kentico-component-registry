import React, { useMemo, useState } from 'react';
import { Search } from 'lucide-react';
import { EmailConfigurationListItem } from './EmailConfigurationListItem';
import type { EmailConfigurationUsageDto } from './types';

interface EmailConfigurationListProps {
  emailConfigurations: EmailConfigurationUsageDto[];
  inspectedComponentIdentifier: string;
  inspectedComponentType: string;
  inspectedComponentTypeName?: string;
}

export const EmailConfigurationList: React.FC<EmailConfigurationListProps> = ({
  emailConfigurations,
  inspectedComponentIdentifier,
  inspectedComponentType,
  inspectedComponentTypeName,
}) => {
  const [searchTerm, setSearchTerm] = useState('');

  const filteredConfigurations = useMemo(() => {
    if (!searchTerm.trim()) {
      return emailConfigurations;
    }
    return emailConfigurations.filter((config) =>
      config.configurationName.toLowerCase().includes(searchTerm.toLowerCase()),
    );
  }, [emailConfigurations, searchTerm]);

  if (emailConfigurations.length === 0) {
    return (
      <div className="p-4 bg-slate-50 rounded-lg border border-slate-200">
        <p className="text-sm text-slate-600">
          No email configurations are using this component.
        </p>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <div className="relative">
          <Search
            size={16}
            className="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400"
          />
          <input
            type="text"
            placeholder="Search configurations by name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-9 pr-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-slate-500"
          />
        </div>
      </div>

      <h3 className="text-sm font-semibold text-slate-900 mb-3">
        {filteredConfigurations.length} email configuration
        {filteredConfigurations.length !== 1 ? 's' : ''}
      </h3>

      <div className="space-y-3">
        {filteredConfigurations.map((config) => (
          <EmailConfigurationListItem
            key={`${config.emailConfigurationId}-${config.contentItemId}`}
            emailConfiguration={config}
            inspectedComponentIdentifier={inspectedComponentIdentifier}
            inspectedComponentType={inspectedComponentType}
            inspectedComponentTypeName={inspectedComponentTypeName}
          />
        ))}
      </div>
    </div>
  );
};
