import React, { useMemo, useState } from 'react';
import { Search } from 'lucide-react';
import { EmailConfigurationListItem } from './EmailConfigurationListItem';
import type { EmailConfigurationUsageDto } from './types';
import { Input } from '../ui/input';
import { Callout } from '../ui/callout';

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
      <Callout type="info">
        <p>
          No email configurations are using this component.
        </p>
      </Callout>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <div className="relative">
          <Search
            size={16}
            className="absolute left-3 top-1/2 transform -translate-y-1/2 xp-muted-dash"
          />
          <Input
            type="text"
            placeholder="Search configurations by name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-9 pr-3 text-sm"
          />
        </div>
      </div>

      <h3 className="text-sm font-semibold text-slate-900 mb-3">
        {filteredConfigurations.length} email configuration
        {filteredConfigurations.length !== 1 ? 's' : ''}
      </h3>

      <div className="space-y-3">
        {filteredConfigurations.length === 0 ? (
          <Callout type="info">
            No configurations match &quot;{searchTerm}&quot;.
          </Callout>
        ) : (
          filteredConfigurations.map((config) => (
            <EmailConfigurationListItem
              key={`${config.emailConfigurationId}-${config.contentItemId}`}
              emailConfiguration={config}
              inspectedComponentIdentifier={inspectedComponentIdentifier}
              inspectedComponentType={inspectedComponentType}
              inspectedComponentTypeName={inspectedComponentTypeName}
            />
          ))
        )}
      </div>
    </div>
  );
};
