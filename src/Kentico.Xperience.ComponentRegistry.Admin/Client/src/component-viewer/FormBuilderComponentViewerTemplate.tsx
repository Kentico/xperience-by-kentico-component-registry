import React, { useState } from 'react';
import { usePageCommand } from '@kentico/xperience-admin-base';
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
import { Callout } from './ui/callout';
import { ChevronDown, Link, Loader } from 'lucide-react';
import {
  FormComponentDto,
  FormComponentUsageDetailDto,
  FormSectionDto,
} from './ComponentDetails';

interface FormBuilderComponentViewerClientProperties {
  formComponents: FormComponentDto[];
  formSections: FormSectionDto[];
  canViewFormBuilderUsages: boolean;
}

// Table row component for form builder components
const FormComponentTableRow: React.FC<{
  component: FormComponentDto | FormSectionDto;
  componentType: 'component' | 'section';
  canViewFormBuilderUsages: boolean;
}> = ({ component, componentType, canViewFormBuilderUsages }) => {
  const [expanded, setExpanded] = useState(false);
  const [usageData, setUsageData] =
    useState<FormComponentUsageDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { execute: getFormBuilderComponentUsage } = usePageCommand<
    FormComponentUsageDetailDto,
    { componentIdentifier: string }
  >('GetFormBuilderComponentUsage', {
    after: (response) => {
      if (response) setUsageData(response);
      setIsLoading(false);
    },
  });

  const { execute: getFormBuilderSectionUsage } = usePageCommand<
    FormComponentUsageDetailDto,
    { componentIdentifier: string }
  >('GetFormBuilderSectionUsage', {
    after: (response) => {
      if (response) setUsageData(response);
      setIsLoading(false);
    },
  });

  const handleExpandClick = async () => {
    if (!expanded && !usageData) {
      if (!canViewFormBuilderUsages) {
        return;
      }
      setIsLoading(true);
      try {
        const params = { componentIdentifier: component.identifier };
        if (componentType === 'section') {
          await getFormBuilderSectionUsage(params);
        } else {
          await getFormBuilderComponentUsage(params);
        }
      } catch {
        setIsLoading(false);
        // Handle error silently
      }
    }
    setExpanded(!expanded);
  };

  const combinedForms = usageData
    ? (() => {
        const normalizeFormKey = (value: string) =>
          value
            .replace(/^BizForm\./i, '')
            .trim()
            .toLowerCase();

        const formsByKey = new Map<
          string,
          {
            key: string;
            displayName: string;
            codeName: string;
            tableName?: string;
            adminPath?: string;
          }
        >();

        usageData.formClasses.forEach((formClass) => {
          const codeName = formClass.className || formClass.classDisplayName;
          const key = normalizeFormKey(codeName);

          formsByKey.set(key, {
            key,
            displayName: formClass.classDisplayName,
            codeName,
            tableName: formClass.classTableName,
          });
        });

        usageData.formBuilderForms.forEach((form) => {
          const codeName = form.formName || form.formDisplayName;
          const key = normalizeFormKey(codeName);
          const existing = formsByKey.get(key);

          formsByKey.set(key, {
            key,
            displayName: existing?.displayName || form.formDisplayName,
            codeName: existing?.codeName || codeName,
            tableName: existing?.tableName,
            adminPath: form.adminPath || existing?.adminPath,
          });
        });

        return Array.from(formsByKey.values()).sort((a, b) =>
          a.displayName.localeCompare(b.displayName),
        );
      })()
    : [];

  return (
    <>
      <TableRow>
        <TableCell className="w-10">
          <button
            onClick={handleExpandClick}
            disabled={isLoading || !canViewFormBuilderUsages}
            title={
              !canViewFormBuilderUsages
                ? 'Permission required to view component usages'
                : ''
            }
            className="xp-icon-button disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? (
              <Loader size={16} className="xp-icon-muted animate-spin" />
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
      </TableRow>

      {expanded && (
        <TableRow>
          <TableCell colSpan={6} className="p-0">
            <div className="p-4 xp-panel-subtle border-t">
              <div className="bg-white p-4 rounded border border-[#dfdfdf]">
                <h4 className="font-semibold xp-text-default mb-4">Details</h4>

                {/* Component info section */}
                <div className="mb-6 pb-6 border-b">
                  <h5 className="text-sm font-medium xp-text-muted mb-3">
                    Component Information
                  </h5>
                  <dl className="space-y-2 text-sm">
                    <div>
                      <dt className="font-medium xp-text-muted">Identifier</dt>
                      <dd className="xp-text-muted font-mono">
                        {component.identifier}
                      </dd>
                    </div>
                    <div>
                      <dt className="font-medium xp-text-muted">Name</dt>
                      <dd className="xp-text-muted">{component.name}</dd>
                    </div>
                    {component.description && (
                      <div>
                        <dt className="font-medium xp-text-muted">
                          Description
                        </dt>
                        <dd className="xp-text-muted">
                          {component.description}
                        </dd>
                      </div>
                    )}
                    {component.markedTypeName && (
                      <div>
                        <dt className="font-medium xp-text-muted">
                          Component Type
                        </dt>
                        <dd className="xp-text-muted font-mono text-xs break-all">
                          {component.markedTypeName}
                        </dd>
                      </div>
                    )}
                  </dl>
                </div>

                {/* Usage section */}
                <div>
                  <h5 className="text-sm font-medium xp-text-muted mb-4">
                    Component Usage
                  </h5>
                  {usageData ? (
                    <div className="space-y-6">
                      <div>
                        <h6 className="text-sm font-medium xp-text-muted mb-3">
                          Form Builder
                        </h6>
                        <div className="space-y-3">
                          <div className="grid grid-cols-2 gap-4 text-sm">
                            <div className="bg-[#f5f5f5] p-3 rounded border border-[#dfdfdf]">
                              <div className="text-xs xp-text-muted font-medium">
                                Total Forms
                              </div>
                              <div className="text-xl font-bold xp-text-default">
                                {combinedForms.length}
                              </div>
                            </div>
                            <div className="bg-[#f5f5f5] p-3 rounded border border-[#dfdfdf]">
                              <div className="text-xs xp-text-muted font-medium">
                                Last Updated
                              </div>
                              <div className="text-sm xp-text-muted">
                                {usageData.lastModified
                                  ? new Date(
                                      usageData.lastModified,
                                    ).toLocaleDateString()
                                  : 'N/A'}
                              </div>
                            </div>
                          </div>

                          {combinedForms.length > 0 ? (
                            <div className="mt-4">
                              <div className="text-xs font-medium xp-text-muted mb-2">
                                Forms:
                              </div>
                              <div className="space-y-2 max-h-64 overflow-y-auto pr-3">
                                {combinedForms.map((form) => (
                                  <div
                                    key={form.key}
                                    className="p-3 bg-[#fbfbfb] rounded border border-[#dfdfdf] text-xs flex items-center justify-between gap-3"
                                  >
                                    <div className="min-w-0 flex-1">
                                      <div className="font-medium xp-text-default mb-1">
                                        {form.displayName}
                                      </div>
                                      <div className="font-mono xp-text-muted text-xs">
                                        {form.codeName}
                                      </div>
                                      {form.tableName && (
                                        <div className="xp-empty-text text-xs mt-1">
                                          Table: {form.tableName}
                                        </div>
                                      )}
                                    </div>
                                    {form.adminPath ? (
                                      <a
                                        href={form.adminPath}
                                        title="Open form in Form Builder"
                                        aria-label="Open form in Form Builder"
                                        className="text-[#3d5dff] hover:text-[#003ddc] flex-shrink-0 mr-2"
                                      >
                                        <Link size={20} />
                                      </a>
                                    ) : null}
                                  </div>
                                ))}
                              </div>
                            </div>
                          ) : (
                            <Callout type="warning">
                              No forms use this item
                            </Callout>
                          )}
                        </div>
                      </div>
                    </div>
                  ) : (
                    <div className="xp-empty-text italic">
                      Loading usage information...
                    </div>
                  )}
                </div>
              </div>
            </div>
          </TableCell>
        </TableRow>
      )}
    </>
  );
};

export const FormBuilderComponentViewerTemplate = (
  props: FormBuilderComponentViewerClientProperties,
) => {
  const [componentFilter, setComponentFilter] = useState('');
  const [sectionFilter, setSectionFilter] = useState('');

  const totalComponents =
    props.formComponents.length + props.formSections.length;

  const filteredFormComponents = props.formComponents.filter((component) =>
    component.identifier
      .toLowerCase()
      .includes(componentFilter.trim().toLowerCase()),
  );
  const filteredFormSections = props.formSections.filter((section) =>
    section.identifier
      .toLowerCase()
      .includes(sectionFilter.trim().toLowerCase()),
  );

  return (
    <div className="min-h-screen p-8">
      <div className="w-full max-w-[110rem] mx-auto space-y-8">
        {/* Header */}
        <div className="space-y-2">
          <Headline size="L">
            Form Builder Components
          </Headline>
          <p className="text-lg xp-text-muted">
            Browse and explore all registered form builder components in the
            system
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
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
                Form Components
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-[#151515]">
                {props.formComponents.length}
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium text-[#6f6f6f]">
                Form Sections
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold text-[#151515]">
                {props.formSections.length}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Tabs */}
        <Tabs defaultValue="components" className="w-full">
          <TabsList className="w-full max-w-md">
            <TabsTrigger value="components">
              Components ({props.formComponents.length})
            </TabsTrigger>
            <TabsTrigger value="sections">
              Sections ({props.formSections.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="components" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Form Component Types
                </CardTitle>
                <CardDescription className="text-base">
                  Reusable components for building forms
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.formComponents.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <Input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={componentFilter}
                        onChange={(e) => setComponentFilter(e.target.value)}
                      />
                    </div>
                    {filteredFormComponents.length > 0 ? (
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
                            {filteredFormComponents.map((component) => (
                              <FormComponentTableRow
                                key={component.identifier}
                                component={component}
                                componentType="component"
                                canViewFormBuilderUsages={
                                  props.canViewFormBuilderUsages
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
                    <p className="text-lg">No form components registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="sections" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle className="text-2xl">
                  Form Section Types
                </CardTitle>
                <CardDescription className="text-base">
                  Layout sections for organizing form components
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.formSections.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <Input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={sectionFilter}
                        onChange={(e) => setSectionFilter(e.target.value)}
                      />
                    </div>
                    {filteredFormSections.length > 0 ? (
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
                            {filteredFormSections.map((section) => (
                              <FormComponentTableRow
                                key={section.identifier}
                                component={section}
                                componentType="section"
                                canViewFormBuilderUsages={
                                  props.canViewFormBuilderUsages
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
                    <p className="text-lg">No form sections registered</p>
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
