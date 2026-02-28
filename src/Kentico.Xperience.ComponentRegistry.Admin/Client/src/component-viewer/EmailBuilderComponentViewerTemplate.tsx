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
            className="p-1 hover:bg-slate-100 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {!usageData && expanded ? (
              <Loader size={16} className="animate-spin text-slate-600" />
            ) : (
              <ChevronDown
                size={16}
                className={`text-slate-600 transition-transform ${
                  expanded ? '-rotate-180' : ''
                }`}
              />
            )}
          </button>
        </TableCell>
        <TableCell>
          <code className="px-2 py-1 bg-slate-100 rounded text-xs font-mono text-slate-700">
            {component.identifier}
          </code>
        </TableCell>
        <TableCell className="font-semibold text-slate-900">
          {component.name}
        </TableCell>
        <TableCell className="text-slate-600 max-w-md">
          {component.description || (
            <span className="text-slate-400 italic">No description</span>
          )}
        </TableCell>
        <TableCell>
          {component.iconClass ? (
            <code className="px-2 py-1 bg-blue-50 rounded text-xs font-mono text-blue-700">
              {component.iconClass}
            </code>
          ) : (
            <span className="text-slate-400">—</span>
          )}
        </TableCell>
        <TableCell>
          {component.markedTypeName ? (
            <div
              className="max-w-44 overflow-x-auto overflow-y-hidden"
              title={component.markedTypeName}
            >
              <code className="px-2 py-1 bg-purple-50 rounded text-xs font-mono text-purple-700 whitespace-nowrap">
                {component.markedTypeName}
              </code>
            </div>
          ) : (
            <span className="text-slate-400">—</span>
          )}
        </TableCell>
        {isTemplate && (
          <TableCell>
            {component.contentTypeNames.length > 0 ? (
              <div className="flex flex-wrap gap-1">
                {component.contentTypeNames.map((ct: string) => (
                  <span
                    key={ct}
                    className="inline-block px-2 py-1 bg-indigo-100 text-indigo-700 rounded-full text-xs font-medium"
                  >
                    {ct}
                  </span>
                ))}
              </div>
            ) : (
              <span className="text-slate-400">—</span>
            )}
          </TableCell>
        )}
      </TableRow>

      {expanded && (
        <TableRow>
          <TableCell colSpan={isTemplate ? 7 : 6} className="p-0">
            <div className="p-4 bg-slate-50 border-t">
              {usageData ? (
                <ComponentDetailsPanel
                  data={usageData}
                  inspectedComponentTypeName={component.markedTypeName}
                />
              ) : expanded && !usageData ? (
                <div className="flex items-center justify-center p-8">
                  <Loader className="animate-spin text-slate-600 mr-2" />
                  <span className="text-slate-600">Loading usage data...</span>
                </div>
              ) : (
                <div className="text-center p-8 text-slate-500">
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
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 p-8 overflow-x-hidden">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* Header */}
        <div className="space-y-2">
          <h1 className="text-4xl font-bold tracking-tight !text-slate-900">
            Email Builder Components
          </h1>
          <p className="text-lg !text-slate-600">
            Browse and explore all registered email builder components in the
            system
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <Card className="bg-gradient-to-br from-blue-50 to-blue-100 border-blue-200">
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium !text-blue-700">
                Total Components
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold !text-blue-900">
                {totalComponents}
              </div>
            </CardContent>
          </Card>

          <Card className="bg-gradient-to-br from-purple-50 to-purple-100 border-purple-200">
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium !text-purple-700">
                Widgets
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold !text-purple-900">
                {props.widgets.length}
              </div>
            </CardContent>
          </Card>

          <Card className="bg-gradient-to-br from-green-50 to-green-100 border-green-200">
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium !text-green-700">
                Sections
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold !text-green-900">
                {props.sections.length}
              </div>
            </CardContent>
          </Card>

          <Card className="bg-gradient-to-br from-orange-50 to-orange-100 border-orange-200">
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium !text-orange-700">
                Email Templates
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold !text-orange-900">
                {props.emailTemplates.length}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Tabs */}
        <Tabs defaultValue="widgets" className="w-full">
          <TabsList className="grid w-full max-w-md grid-cols-3">
            <TabsTrigger
              value="widgets"
              className="!text-slate-700 data-[state=active]:!text-slate-900"
            >
              Widgets ({props.widgets.length})
            </TabsTrigger>
            <TabsTrigger
              value="sections"
              className="!text-slate-700 data-[state=active]:!text-slate-900"
            >
              Sections ({props.sections.length})
            </TabsTrigger>
            <TabsTrigger
              value="emailTemplates"
              className="!text-slate-700 data-[state=active]:!text-slate-900"
            >
              Templates ({props.emailTemplates.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="widgets" className="space-y-4">
            <Card className="shadow-lg">
              <CardHeader className="bg-gradient-to-r from-purple-50 to-blue-50">
                <CardTitle className="text-2xl !text-slate-900">
                  Email Widget Components
                </CardTitle>
                <CardDescription className="text-base !text-slate-600">
                  Reusable widgets for email builder
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.widgets.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={widgetFilter}
                        onChange={(e) => setWidgetFilter(e.target.value)}
                        className="w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-slate-500"
                      />
                    </div>
                    {filteredWidgets.length > 0 ? (
                      <div className="rounded-lg border">
                        <Table>
                          <TableHeader>
                            <TableRow className="bg-slate-50">
                              <TableHead className="w-10"></TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Identifier
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Name
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Description
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Icon
                              </TableHead>
                              <TableHead className="w-44 font-semibold !text-slate-700">
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
                      <div className="text-center py-8 text-slate-500">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 text-slate-500">
                    <p className="text-lg">No email widgets registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="sections" className="space-y-4">
            <Card className="shadow-lg">
              <CardHeader className="bg-gradient-to-r from-green-50 to-teal-50">
                <CardTitle className="text-2xl !text-slate-900">
                  Email Section Components
                </CardTitle>
                <CardDescription className="text-base !text-slate-600">
                  Layout sections for structuring email content
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.sections.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={sectionFilter}
                        onChange={(e) => setSectionFilter(e.target.value)}
                        className="w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-slate-500"
                      />
                    </div>
                    {filteredSections.length > 0 ? (
                      <div className="rounded-lg border">
                        <Table>
                          <TableHeader>
                            <TableRow className="bg-slate-50">
                              <TableHead className="w-10"></TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Identifier
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Name
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Description
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Icon
                              </TableHead>
                              <TableHead className="w-44 font-semibold !text-slate-700">
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
                      <div className="text-center py-8 text-slate-500">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 text-slate-500">
                    <p className="text-lg">No email sections registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="emailTemplates" className="space-y-4">
            <Card className="shadow-lg">
              <CardHeader className="bg-gradient-to-r from-orange-50 to-amber-50">
                <CardTitle className="text-2xl !text-slate-900">
                  Email Template Components
                </CardTitle>
                <CardDescription className="text-base !text-slate-600">
                  Complete email templates for different content types
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.emailTemplates.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={templateFilter}
                        onChange={(e) => setTemplateFilter(e.target.value)}
                        className="w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-slate-500"
                      />
                    </div>
                    {filteredTemplates.length > 0 ? (
                      <div className="rounded-lg border">
                        <Table>
                          <TableHeader>
                            <TableRow className="bg-slate-50">
                              <TableHead className="w-10"></TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Identifier
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Name
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Description
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
                                Icon
                              </TableHead>
                              <TableHead className="w-44 font-semibold !text-slate-700">
                                Component Type
                              </TableHead>
                              <TableHead className="font-semibold !text-slate-700">
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
                      <div className="text-center py-8 text-slate-500">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 text-slate-500">
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
