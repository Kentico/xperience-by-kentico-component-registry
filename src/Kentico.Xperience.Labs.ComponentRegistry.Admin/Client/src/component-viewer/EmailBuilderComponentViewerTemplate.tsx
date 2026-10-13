import React, { useState } from 'react';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from './ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from './ui/table';
import { Input } from './ui/input';
import { IconTableCell, IconTableHead } from './ui/icon-table-cell';
import { ChevronDown, Loader } from 'lucide-react';
import { usePageCommand } from '@kentico/xperience-admin-base';
import { ComponentDetailsPanel } from './ComponentDetails';
import {
  EmailComponentDto,
  EmailConfigurationUsageDetailDto,
  EmailTemplateDto,
} from './ComponentDetails/types';

type EmailBuilderComponentKind = 'widget' | 'section' | 'template';

interface EmailBuilderComponentViewerClientProperties {
  components: (EmailComponentDto | EmailTemplateDto)[];
  componentKind: EmailBuilderComponentKind;
  canViewUsages: boolean;
}

const viewConfig: Record<
  EmailBuilderComponentKind,
  { title: string; description: string; emptyText: string }
> = {
  widget: {
    title: 'Email Widget Components',
    description: 'Reusable widgets for email builder',
    emptyText: 'No email widgets registered',
  },
  section: {
    title: 'Email Section Components',
    description: 'Layout sections for structuring email content',
    emptyText: 'No email sections registered',
  },
  template: {
    title: 'Email Template Components',
    description: 'Complete email templates for different content types',
    emptyText: 'No email templates registered',
  },
};

// Table row component for email builder components with expandable details
const EmailComponentTableRow: React.FC<{
  component: EmailComponentDto | EmailTemplateDto;
  showContentTypes: boolean;
  canViewUsages: boolean;
}> = ({ component, showContentTypes, canViewUsages }) => {
  const [expanded, setExpanded] = useState(false);
  const [usageData, setUsageData] =
    useState<EmailConfigurationUsageDetailDto | null>(null);

  const { execute: getComponentUsage } = usePageCommand<
    EmailConfigurationUsageDetailDto,
    { componentIdentifier: string }
  >('GetEmailBuilderComponentUsage', {
    after: (response) => {
      if (response) setUsageData(response);
    },
  });

  const handleExpandClick = async () => {
    if (!expanded && !usageData) {
      if (!canViewUsages) {
        return;
      }
      try {
        await getComponentUsage({ componentIdentifier: component.identifier });
      } catch (error) {
        // eslint-disable-next-line no-console
        console.error('Failed to fetch usage data:', error);
      }
    }
    setExpanded(!expanded);
  };

  const contentTypeNames =
    showContentTypes && 'contentTypeNames' in component
      ? component.contentTypeNames
      : undefined;

  return (
    <>
      <TableRow>
        <TableCell className="w-10">
          <button
            onClick={handleExpandClick}
            disabled={!canViewUsages || (!usageData && expanded)}
            title={
              !canViewUsages
                ? 'Permission required to view component usages'
                : ''
            }
            className="xp-icon-button disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {!usageData && expanded ? (
              <Loader size={16} className="animate-spin xp-icon-muted" />
            ) : (
              <ChevronDown
                size={16}
                className={`xp-icon-muted transition-transform ${
                  expanded ? '-rotate-180' : ''
                }`}
              />
            )}
          </button>
        </TableCell>
        <TableCell>
          <code className="xp-code-chip">{component.identifier}</code>
        </TableCell>
        <IconTableCell iconClass={component.iconClass} />
        <TableCell className="font-semibold xp-text-default">
          {component.name}
        </TableCell>
        <TableCell className="xp-text-muted max-w-md">
          {component.description || (
            <span className="xp-muted-dash italic">No description</span>
          )}
        </TableCell>
        <TableCell>
          {component.markedTypeName ? (
            <div
              className="max-w-xs overflow-x-auto overflow-y-hidden"
              title={component.markedTypeName}
            >
              <code className="xp-code-chip whitespace-nowrap">
                {component.markedTypeName}
              </code>
            </div>
          ) : (
            <span className="xp-muted-dash">—</span>
          )}
        </TableCell>
        {showContentTypes && (
          <TableCell>
            {contentTypeNames && contentTypeNames.length > 0 ? (
              <div className="flex flex-wrap gap-1">
                {contentTypeNames.map((ct) => (
                  <span key={ct} className="xp-tag">
                    {ct}
                  </span>
                ))}
              </div>
            ) : (
              <span className="xp-muted-dash">—</span>
            )}
          </TableCell>
        )}
      </TableRow>

      {expanded && (
        <TableRow>
          <TableCell colSpan={showContentTypes ? 7 : 6} className="p-0">
            <div className="p-4 xp-panel-subtle border-t">
              {usageData ? (
                <ComponentDetailsPanel
                  data={usageData}
                  inspectedComponentTypeName={component.markedTypeName}
                />
              ) : (
                <div className="flex items-center justify-center p-8">
                  <Loader className="animate-spin xp-icon-muted mr-2" />
                  <span className="xp-text-muted">Loading usage data...</span>
                </div>
              )}
            </div>
          </TableCell>
        </TableRow>
      )}
    </>
  );
};

export const EmailBuilderComponentViewerTemplate = (
  props: EmailBuilderComponentViewerClientProperties,
) => {
  const [filter, setFilter] = useState('');

  const config = viewConfig[props.componentKind];
  if (!config) {
    return null;
  }

  const components = props.components;
  const showContentTypes = props.componentKind === 'template';

  const normalizedFilter = filter.trim().toLowerCase();
  const filteredComponents = components.filter((component) =>
    component.identifier.toLowerCase().includes(normalizedFilter),
  );

  return (
    <div className="min-h-screen mr-4 overflow-x-hidden">
      <div className="w-full max-w-[110rem] mx-auto">
        <Card>
          <CardHeader>
            <CardTitle className="text-2xl">{config.title}</CardTitle>
            <CardDescription className="text-base">
              {config.description}
            </CardDescription>
          </CardHeader>
          <CardContent className="pt-6">
            {components.length > 0 ? (
              <>
                <div className="mb-4">
                  <Input
                    type="text"
                    placeholder="Filter by identifier..."
                    value={filter}
                    onChange={(e) => setFilter(e.target.value)}
                  />
                </div>
                {filteredComponents.length > 0 ? (
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead className="w-10"></TableHead>
                        <TableHead className="font-semibold">
                          Identifier
                        </TableHead>
                        <IconTableHead />
                        <TableHead className="font-semibold">Name</TableHead>
                        <TableHead className="font-semibold">
                          Description
                        </TableHead>
                        <TableHead className="font-semibold">
                          Component Type
                        </TableHead>
                        {showContentTypes && (
                          <TableHead className="font-semibold">
                            Content Types
                          </TableHead>
                        )}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredComponents.map((component) => (
                        <EmailComponentTableRow
                          key={component.identifier}
                          component={component}
                          showContentTypes={showContentTypes}
                          canViewUsages={props.canViewUsages}
                        />
                      ))}
                    </TableBody>
                  </Table>
                ) : (
                  <div className="text-center py-8 xp-empty-text">
                    <p>No components match this identifier filter</p>
                  </div>
                )}
              </>
            ) : (
              <div className="text-center py-12 xp-empty-text">
                <p className="text-lg">{config.emptyText}</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};
