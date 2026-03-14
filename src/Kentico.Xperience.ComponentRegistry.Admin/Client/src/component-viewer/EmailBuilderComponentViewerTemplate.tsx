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
import { Tabs, TabsContent, TabsList, TabsTrigger } from './ui/tabs';
import { Input } from './ui/input';
import { Headline } from './ui/headline';
import { ChevronDown, Loader } from 'lucide-react';
import { usePageCommand } from '@kentico/xperience-admin-base';
import { ComponentDetailsPanel } from './ComponentDetails';
import {
  EmailComponentDto,
  EmailConfigurationUsageDetailDto,
  EmailTemplateDto,
} from './ComponentDetails/types';

interface EmailBuilderComponentViewerClientProperties {
  widgets: EmailComponentDto[];
  sections: EmailComponentDto[];
  emailTemplates: EmailTemplateDto[];
  canViewEmailBuilderUsages: boolean;
}

// Table row component for email builder components with expandable details
const EmailComponentTableRow: React.FC<{
  component: EmailComponentDto | EmailTemplateDto;
  componentType: 'widget' | 'section' | 'template';
  canViewEmailBuilderUsages: boolean;
}> = ({ component, componentType, canViewEmailBuilderUsages }) => {
  const [expanded, setExpanded] = useState(false);
  const [usageData, setUsageData] =
    useState<EmailConfigurationUsageDetailDto | null>(null);

  // Use page command hooks for fetching usage data
  const { execute: getEmailBuilderWidgetUsage } = usePageCommand<
    EmailConfigurationUsageDetailDto,
    { componentIdentifier: string }
  >('GetEmailBuilderWidgetUsage', {
    after: (response) => {
      if (response) setUsageData(response);
    },
  });
  const { execute: getEmailBuilderTemplateUsage } = usePageCommand<
    EmailConfigurationUsageDetailDto,
    { componentIdentifier: string }
  >('GetEmailBuilderTemplateUsage', {
    after: (response) => {
      if (response) setUsageData(response);
    },
  });

  const handleExpandClick = async () => {
    if (!expanded && !usageData) {
      if (!canViewEmailBuilderUsages) {
        return;
      }
      try {
        const params = { componentIdentifier: component.identifier };
        if (componentType === 'template') {
          await getEmailBuilderTemplateUsage(params);
        } else {
          await getEmailBuilderWidgetUsage(params);
        }
      } catch (error) {
        // eslint-disable-next-line no-console
        console.error('Failed to fetch usage data:', error);
      }
    }
    setExpanded(!expanded);
  };

  const isTemplate =
    componentType === 'template' && 'contentTypeNames' in component;

  return (
    <>
      <TableRow>
        <TableCell className="w-10">
          <button
            onClick={handleExpandClick}
            disabled={!canViewEmailBuilderUsages || (!usageData && expanded)}
            title={
              !canViewEmailBuilderUsages
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
          <code className="xp-code-chip">
            {component.identifier}
          </code>
        </TableCell>
        <TableCell className="font-semibold xp-text-default">
          {component.name}
        </TableCell>
        <TableCell className="xp-text-muted max-w-md">
          {component.description || (
            <span className="xp-muted-dash italic">No description</span>
          )}
        </TableCell>
        <TableCell>
          {component.iconClass ? (
            <code className="xp-code-chip">
              {component.iconClass}
            </code>
          ) : (
            <span className="xp-muted-dash">—</span>
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
        {isTemplate && (
          <TableCell>
            {component.contentTypeNames.length > 0 ? (
              <div className="flex flex-wrap gap-1">
                {component.contentTypeNames.map((ct: string) => (
                  <span
                    key={ct}
                    className="xp-tag"
                  >
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
          <TableCell colSpan={isTemplate ? 7 : 6} className="p-0">
            <div className="p-4 xp-panel-subtle border-t">
              {usageData ? (
                <ComponentDetailsPanel
                  data={usageData}
                  inspectedComponentTypeName={component.markedTypeName}
                />
              ) : expanded && !usageData ? (
                <div className="flex items-center justify-center p-8">
                  <Loader className="animate-spin xp-icon-muted mr-2" />
                  <span className="xp-text-muted">Loading usage data...</span>
                </div>
              ) : (
                <div className="text-center p-8 xp-empty-text">
                  <p>No usage data available</p>
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
  const [widgetFilter, setWidgetFilter] = useState('');
  const [sectionFilter, setSectionFilter] = useState('');
  const [templateFilter, setTemplateFilter] = useState('');

  const totalComponents =
    props.widgets.length + props.sections.length + props.emailTemplates.length;

  const filteredWidgets = props.widgets.filter((widget) =>
    widget.identifier.toLowerCase().includes(widgetFilter.trim().toLowerCase()),
  );
  const filteredSections = props.sections.filter((section) =>
    section.identifier
      .toLowerCase()
      .includes(sectionFilter.trim().toLowerCase()),
  );
  const filteredTemplates = props.emailTemplates.filter((template) =>
    template.identifier
      .toLowerCase()
      .includes(templateFilter.trim().toLowerCase()),
  );

  return (
    <div className="min-h-screen p-8 overflow-x-hidden">
      <div className="w-full max-w-[110rem] mx-auto space-y-8">
        {/* Header */}
        <div className="space-y-2">
          <Headline size="L">
            Email Builder Components
          </Headline>
          <p className="text-lg xp-text-muted">
            Browse and explore all registered email builder components in the
            system
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-[#6f6f6f]">
                Total Components
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-[#151515]">
                {totalComponents}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-[#6f6f6f]">
                Widgets
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-[#151515]">
                {props.widgets.length}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-[#6f6f6f]">
                Sections
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-[#151515]">
                {props.sections.length}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-[#6f6f6f]">
                Email Templates
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-[#151515]">
                {props.emailTemplates.length}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Tabs */}
        <Tabs defaultValue="widgets" className="w-full">
          <TabsList className="w-full max-w-md">
            <TabsTrigger value="widgets">
              Widgets ({props.widgets.length})
            </TabsTrigger>
            <TabsTrigger value="sections">
              Sections ({props.sections.length})
            </TabsTrigger>
            <TabsTrigger value="emailTemplates">
              Templates ({props.emailTemplates.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="widgets" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Email Widget Components
                </CardTitle>
                <CardDescription className="text-base">
                  Reusable widgets for email builder
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.widgets.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <Input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={widgetFilter}
                        onChange={(e) => setWidgetFilter(e.target.value)}
                      />
                    </div>
                    {filteredWidgets.length > 0 ? (
                      <div>
                        <Table>
                          <TableHeader>
                            <TableRow>
                              <TableHead className="w-10"></TableHead>
                              <TableHead className="font-semibold">
                                Identifier
                              </TableHead>
                              <TableHead className="font-semibold">
                                Name
                              </TableHead>
                              <TableHead className="font-semibold">
                                Description
                              </TableHead>
                              <TableHead className="font-semibold">
                                Icon
                              </TableHead>
                              <TableHead className="font-semibold">
                                Component Type
                              </TableHead>
                            </TableRow>
                          </TableHeader>
                          <TableBody>
                            {filteredWidgets.map((widget, _index) => (
                              <EmailComponentTableRow
                                key={widget.identifier}
                                component={widget}
                                componentType="widget"
                                canViewEmailBuilderUsages={
                                  props.canViewEmailBuilderUsages
                                }
                              />
                            ))}
                          </TableBody>
                        </Table>
                      </div>
                    ) : (
                      <div className="text-center py-8 xp-empty-text">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 xp-empty-text">
                    <p className="text-lg">No email widgets registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="sections" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Email Section Components
                </CardTitle>
                <CardDescription className="text-base">
                  Layout sections for structuring email content
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.sections.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <Input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={sectionFilter}
                        onChange={(e) => setSectionFilter(e.target.value)}
                      />
                    </div>
                    {filteredSections.length > 0 ? (
                      <div>
                        <Table>
                          <TableHeader>
                            <TableRow>
                              <TableHead className="w-10"></TableHead>
                              <TableHead className="font-semibold">
                                Identifier
                              </TableHead>
                              <TableHead className="font-semibold">
                                Name
                              </TableHead>
                              <TableHead className="font-semibold">
                                Description
                              </TableHead>
                              <TableHead className="font-semibold">
                                Icon
                              </TableHead>
                              <TableHead className="font-semibold">
                                Component Type
                              </TableHead>
                            </TableRow>
                          </TableHeader>
                          <TableBody>
                            {filteredSections.map((section, _index) => (
                              <EmailComponentTableRow
                                key={section.identifier}
                                component={section}
                                componentType="section"
                                canViewEmailBuilderUsages={
                                  props.canViewEmailBuilderUsages
                                }
                              />
                            ))}
                          </TableBody>
                        </Table>
                      </div>
                    ) : (
                      <div className="text-center py-8 xp-empty-text">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 xp-empty-text">
                    <p className="text-lg">No email sections registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="emailTemplates" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Email Template Components
                </CardTitle>
                <CardDescription className="text-base">
                  Complete email templates for different content types
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.emailTemplates.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <Input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={templateFilter}
                        onChange={(e) => setTemplateFilter(e.target.value)}
                      />
                    </div>
                    {filteredTemplates.length > 0 ? (
                      <div>
                        <Table>
                          <TableHeader>
                            <TableRow>
                              <TableHead className="w-10"></TableHead>
                              <TableHead className="font-semibold">
                                Identifier
                              </TableHead>
                              <TableHead className="font-semibold">
                                Name
                              </TableHead>
                              <TableHead className="font-semibold">
                                Description
                              </TableHead>
                              <TableHead className="font-semibold">
                                Icon
                              </TableHead>
                              <TableHead className="font-semibold">
                                Component Type
                              </TableHead>
                              <TableHead className="font-semibold">
                                Content Types
                              </TableHead>
                            </TableRow>
                          </TableHeader>
                          <TableBody>
                            {filteredTemplates.map((template, _index) => (
                              <EmailComponentTableRow
                                key={template.identifier}
                                component={template}
                                componentType="template"
                                canViewEmailBuilderUsages={
                                  props.canViewEmailBuilderUsages
                                }
                              />
                            ))}
                          </TableBody>
                        </Table>
                      </div>
                    ) : (
                      <div className="text-center py-8 xp-empty-text">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 xp-empty-text">
                    <p className="text-lg">No email templates registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      </div>
    </div>
  );
};
