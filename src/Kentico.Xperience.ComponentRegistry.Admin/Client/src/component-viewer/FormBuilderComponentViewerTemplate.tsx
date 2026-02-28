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
            className="p-1 hover:bg-slate-100 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? (
              <Loader size={16} className="text-slate-600 animate-spin" />
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
              className="max-w-xs overflow-x-auto overflow-y-hidden"
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
      </TableRow>

      {expanded && (
        <TableRow>
          <TableCell colSpan={6} className="p-0">
            <div className="p-4 bg-slate-50 border-t">
              <div className="bg-white p-4 rounded border border-slate-200">
                <h4 className="font-semibold text-slate-900 mb-4">Details</h4>

                {/* Component info section */}
                <div className="mb-6 pb-6 border-b">
                  <h5 className="text-sm font-medium text-slate-700 mb-3">
                    Component Information
                  </h5>
                  <dl className="space-y-2 text-sm">
                    <div>
                      <dt className="font-medium text-slate-700">Identifier</dt>
                      <dd className="text-slate-600 font-mono">
                        {component.identifier}
                      </dd>
                    </div>
                    <div>
                      <dt className="font-medium text-slate-700">Name</dt>
                      <dd className="text-slate-600">{component.name}</dd>
                    </div>
                    {component.description && (
                      <div>
                        <dt className="font-medium text-slate-700">
                          Description
                        </dt>
                        <dd className="text-slate-600">
                          {component.description}
                        </dd>
                      </div>
                    )}
                    {component.markedTypeName && (
                      <div>
                        <dt className="font-medium text-slate-700">
                          Component Type
                        </dt>
                        <dd className="text-slate-600 font-mono text-xs break-all">
                          {component.markedTypeName}
                        </dd>
                      </div>
                    )}
                  </dl>
                </div>

                {/* Usage section */}
                <div>
                  <h5 className="text-sm font-medium text-slate-700 mb-4">
                    Component Usage
                  </h5>
                  {usageData ? (
                    <div className="space-y-6">
                      <div>
                        <h6 className="text-sm font-medium text-slate-600 mb-3">
                          Form Builder
                        </h6>
                        <div className="space-y-3">
                          <div className="grid grid-cols-2 gap-4 text-sm">
                            <div className="bg-blue-50 p-3 rounded">
                              <div className="text-xs text-blue-700 font-medium">
                                Total Forms
                              </div>
                              <div className="text-xl font-bold text-blue-900">
                                {combinedForms.length}
                              </div>
                            </div>
                            <div className="bg-slate-100 p-3 rounded">
                              <div className="text-xs text-slate-700 font-medium">
                                Last Updated
                              </div>
                              <div className="text-sm text-slate-600">
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
                              <div className="text-xs font-medium text-slate-700 mb-2">
                                Forms:
                              </div>
                              <div className="space-y-2 max-h-64 overflow-y-auto pr-3">
                                {combinedForms.map((form) => (
                                  <div
                                    key={form.key}
                                    className="p-3 bg-slate-50 rounded border border-slate-200 text-xs flex items-center justify-between gap-3"
                                  >
                                    <div className="min-w-0 flex-1">
                                      <div className="font-medium text-slate-900 mb-1">
                                        {form.displayName}
                                      </div>
                                      <div className="font-mono text-slate-600 text-xs">
                                        {form.codeName}
                                      </div>
                                      {form.tableName && (
                                        <div className="text-slate-500 text-xs mt-1">
                                          Table: {form.tableName}
                                        </div>
                                      )}
                                    </div>
                                    {form.adminPath ? (
                                      <a
                                        href={form.adminPath}
                                        title="Open form in Form Builder"
                                        aria-label="Open form in Form Builder"
                                        className="text-blue-700 hover:text-blue-900 flex-shrink-0 mr-2"
                                      >
                                        <Link size={20} />
                                      </a>
                                    ) : null}
                                  </div>
                                ))}
                              </div>
                            </div>
                          ) : (
                            <div className="p-3 bg-yellow-50 rounded text-sm text-yellow-700">
                              No forms use this item
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  ) : (
                    <div className="text-slate-500 italic">
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
    <div className="min-h-screen bg-gradient-to-br from-slate-50 to-slate-100 p-8">
      <div className="max-w-7xl mx-auto space-y-8">
        {/* Header */}
        <div className="space-y-2">
          <h1 className="text-4xl font-bold tracking-tight !text-slate-900">
            Form Builder Components
          </h1>
          <p className="text-lg !text-slate-600">
            Browse and explore all registered form builder components in the
            system
          </p>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
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
                Form Components
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold !text-purple-900">
                {props.formComponents.length}
              </div>
            </CardContent>
          </Card>

          <Card className="bg-gradient-to-br from-green-50 to-green-100 border-green-200">
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium !text-green-700">
                Form Sections
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="text-3xl font-bold !text-green-900">
                {props.formSections.length}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Tabs */}
        <Tabs defaultValue="components" className="w-full">
          <TabsList className="grid w-full max-w-md grid-cols-2">
            <TabsTrigger
              value="components"
              className="!text-slate-700 data-[state=active]:!text-slate-900"
            >
              Components ({props.formComponents.length})
            </TabsTrigger>
            <TabsTrigger
              value="sections"
              className="!text-slate-700 data-[state=active]:!text-slate-900"
            >
              Sections ({props.formSections.length})
            </TabsTrigger>
          </TabsList>

          <TabsContent value="components" className="space-y-4">
            <Card className="shadow-lg">
              <CardHeader className="bg-gradient-to-r from-purple-50 to-blue-50">
                <CardTitle className="text-2xl !text-slate-900">
                  Form Component Types
                </CardTitle>
                <CardDescription className="text-base !text-slate-600">
                  Reusable components for building forms
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.formComponents.length > 0 ? (
                  <>
                    <div className="mb-4">
                      <input
                        type="text"
                        placeholder="Filter by identifier..."
                        value={componentFilter}
                        onChange={(e) => setComponentFilter(e.target.value)}
                        className="w-full px-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-slate-500"
                      />
                    </div>
                    {filteredFormComponents.length > 0 ? (
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
                              <TableHead className="font-semibold !text-slate-700">
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
                      <div className="text-center py-8 text-slate-500">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 text-slate-500">
                    <p className="text-lg">No form components registered</p>
                  </div>
                )}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="sections" className="space-y-4">
            <Card className="shadow-lg">
              <CardHeader className="bg-gradient-to-r from-green-50 to-teal-50">
                <CardTitle className="text-2xl !text-slate-900">
                  Form Section Types
                </CardTitle>
                <CardDescription className="text-base !text-slate-600">
                  Layout sections for organizing form components
                </CardDescription>
              </CardHeader>
              <CardContent className="pt-6">
                {props.formSections.length > 0 ? (
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
                    {filteredFormSections.length > 0 ? (
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
                              <TableHead className="font-semibold !text-slate-700">
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
                      <div className="text-center py-8 text-slate-500">
                        <p>No components match this identifier filter</p>
                      </div>
                    )}
                  </>
                ) : (
                  <div className="text-center py-12 text-slate-500">
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
