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
  ComponentDto,
  ComponentUsageDetailDto,
  PageTemplateDto,
} from './ComponentDetails/types';

interface PageBuilderComponentViewerClientProperties {
  widgets: ComponentDto[];
  sections: ComponentDto[];
  pageTemplates: PageTemplateDto[];
  canViewPageBuilderUsages: boolean;
}

// Table row component with expandable details
const ComponentTableRow: React.FC<{
  component: ComponentDto | PageTemplateDto;
  componentType: 'widget' | 'section' | 'template';
  canViewPageBuilderUsages: boolean;
}> = ({ component, componentType, canViewPageBuilderUsages }) => {
  const [expanded, setExpanded] = useState(false);
  const [usageData, setUsageData] = useState<ComponentUsageDetailDto | null>(
    null,
  );

  // Use page command hooks for fetching usage data
  const { execute: getPageBuilderPageTemplateUsage } = usePageCommand<
    ComponentUsageDetailDto,
    { componentIdentifier: string }
  >('GetPageBuilderPageTemplateUsage', {
    after: (response) => {
      if (response) setUsageData(response);
    },
  });
  const { execute: getPageBuilderWidgetUsage } = usePageCommand<
    ComponentUsageDetailDto,
    { componentIdentifier: string }
  >('GetPageBuilderWidgetUsage', {
    after: (response) => {
      if (response) setUsageData(response);
    },
  });

  const handleExpandClick = async () => {
    if (!expanded && !usageData) {
      if (!canViewPageBuilderUsages) {
        return;
      }
      try {
        const params = { componentIdentifier: component.identifier };
        if (componentType === 'template') {
          await getPageBuilderPageTemplateUsage(params);
        } else {
          await getPageBuilderWidgetUsage(params);
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
            disabled={!canViewPageBuilderUsages || (!usageData && expanded)}
            title={
              !canViewPageBuilderUsages
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
                {component.contentTypeNames.map((ct) => (
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

export const PageBuilderComponentViewerTemplate = (
  props: PageBuilderComponentViewerClientProperties,
) => {
  const [widgetFilter, setWidgetFilter] = useState('');
  const [sectionFilter, setSectionFilter] = useState('');
  const [templateFilter, setTemplateFilter] = useState('');

  const filteredWidgets = props.widgets.filter((widget) =>
    widget.identifier.toLowerCase().includes(widgetFilter.trim().toLowerCase()),
  );
  const filteredSections = props.sections.filter((section) =>
    section.identifier
      .toLowerCase()
      .includes(sectionFilter.trim().toLowerCase()),
  );
  const filteredTemplates = props.pageTemplates.filter((template) =>
    template.identifier
      .toLowerCase()
      .includes(templateFilter.trim().toLowerCase()),
  );

  return (
    <div className="min-h-screen p-8">
      <div className="w-full max-w-[110rem] mx-auto space-y-8">
        {/* Header */}
        <div className="space-y-2">
          <Headline size="L">
            Component Registry
          </Headline>
          <p className="text-lg xp-text-muted">
            Browse and explore all registered components in the system
          </p>
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
            <TabsTrigger value="pageTemplates">
              Templates ({props.pageTemplates.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="widgets" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Widget Components
                </CardTitle>
                <CardDescription className="text-base">
                  Reusable UI widgets for page building
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
                              <ComponentTableRow
                                key={widget.identifier}
                                component={widget}
                                componentType="widget"
                                canViewPageBuilderUsages={
                                  props.canViewPageBuilderUsages
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
                    <p className="text-lg">No widgets registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="sections" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Section Components
                </CardTitle>
                <CardDescription className="text-base">
                  Layout sections for structuring page content
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
                              <ComponentTableRow
                                key={section.identifier}
                                component={section}
                                componentType="section"
                                canViewPageBuilderUsages={
                                  props.canViewPageBuilderUsages
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
                    <p className="text-lg">No sections registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="pageTemplates" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Page Template Components
                </CardTitle>
                <CardDescription className="text-base">
                  Complete page layouts for different content types
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.pageTemplates.length > 0 ? (
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
                              <ComponentTableRow
                                key={template.identifier}
                                component={template}
                                componentType="template"
                                canViewPageBuilderUsages={
                                  props.canViewPageBuilderUsages
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
                    <p className="text-lg">No page templates registered</p>
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
