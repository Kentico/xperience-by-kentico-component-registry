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
import { Input } from './ui/input';
import { IconTableCell, IconTableHead } from './ui/icon-table-cell';
import { Callout } from './ui/callout';
import { ChevronDown, Link, Loader } from 'lucide-react';
import {
  FormComponentDto,
  FormComponentUsageDetailDto,
  FormSectionDto,
} from './ComponentDetails';

type FormBuilderComponentKind = 'component' | 'section';

interface FormBuilderComponentViewerClientProperties {
  components: (FormComponentDto | FormSectionDto)[];
  componentKind: FormBuilderComponentKind;
  canViewUsages: boolean;
}

const viewConfig: Record<
  FormBuilderComponentKind,
  { title: string; description: string; emptyText: string }
> = {
  component: {
    title: 'Form Component Types',
    description: 'Reusable components for building forms',
    emptyText: 'No form components registered',
  },
  section: {
    title: 'Form Section Types',
    description: 'Layout sections for organizing form components',
    emptyText: 'No form sections registered',
  },
};

const formatDate = (value: string) => new Date(value).toLocaleString();

interface CombinedFormUsage {
  key: string;
  displayName: string;
  codeName: string;
  tableName?: string;
  adminPath?: string;
  formLastModified?: string;
  classLastModified?: string;
}

const combineForms = (
  usageData: FormComponentUsageDetailDto,
): CombinedFormUsage[] => {
  const normalizeFormKey = (value: string) =>
    value
      .replace(/^BizForm\./i, '')
      .trim()
      .toLowerCase();

  const formsByKey = new Map<string, CombinedFormUsage>();

  usageData.formClasses.forEach((formClass) => {
    const codeName = formClass.className || formClass.classDisplayName;
    const key = normalizeFormKey(codeName);

    formsByKey.set(key, {
      key,
      displayName: formClass.classDisplayName,
      codeName,
      tableName: formClass.classTableName,
      classLastModified: formClass.classLastModified,
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
      formLastModified: form.formLastModified,
      classLastModified: existing?.classLastModified,
    });
  });

  return Array.from(formsByKey.values()).sort((a, b) =>
    a.displayName.localeCompare(b.displayName),
  );
};

// Table row component for form builder components
const FormComponentTableRow: React.FC<{
  component: FormComponentDto | FormSectionDto;
  canViewUsages: boolean;
}> = ({ component, canViewUsages }) => {
  const [expanded, setExpanded] = useState(false);
  const [usageData, setUsageData] =
    useState<FormComponentUsageDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { execute: getComponentUsage } = usePageCommand<
    FormComponentUsageDetailDto,
    { componentIdentifier: string }
  >('GetFormBuilderComponentUsage', {
    after: (response) => {
      if (response) setUsageData(response);
      setIsLoading(false);
    },
  });

  const handleExpandClick = async () => {
    if (!expanded && !usageData) {
      if (!canViewUsages) {
        return;
      }
      setIsLoading(true);
      try {
        await getComponentUsage({ componentIdentifier: component.identifier });
      } catch {
        setIsLoading(false);
      }
    }
    setExpanded(!expanded);
  };

  const combinedForms = usageData ? combineForms(usageData) : [];

  return (
    <>
      <TableRow>
        <TableCell className="w-10">
          <button
            onClick={handleExpandClick}
            disabled={isLoading || !canViewUsages}
            title={
              !canViewUsages
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
                  <dl className="text-sm space-y-3">
                    <div className="grid gap-1 sm:grid-cols-[11rem_1fr] sm:gap-4 sm:items-start">
                      <dt className="font-medium xp-text-muted">Identifier</dt>
                      <dd className="xp-text-muted font-mono break-all">
                        {component.identifier}
                      </dd>
                    </div>
                    <div className="grid gap-1 sm:grid-cols-[11rem_1fr] sm:gap-4 sm:items-start">
                      <dt className="font-medium xp-text-muted">Name</dt>
                      <dd className="xp-text-muted break-words">
                        {component.name}
                      </dd>
                    </div>
                    {component.description && (
                      <div className="grid gap-1 sm:grid-cols-[11rem_1fr] sm:gap-4 sm:items-start">
                        <dt className="font-medium xp-text-muted">
                          Description
                        </dt>
                        <dd className="xp-text-muted break-words">
                          {component.description}
                        </dd>
                      </div>
                    )}
                    {component.markedTypeName && (
                      <div className="grid gap-1 sm:grid-cols-[11rem_1fr] sm:gap-4 sm:items-start">
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
                                      {form.formLastModified && (
                                        <div className="xp-text-muted text-xs mt-1">
                                          Form updated:{' '}
                                          {formatDate(form.formLastModified)}
                                        </div>
                                      )}
                                      {form.classLastModified && (
                                        <div className="xp-text-muted text-xs mt-1">
                                          Form class updated:{' '}
                                          {formatDate(form.classLastModified)}
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
  const [filter, setFilter] = useState('');

  const config = viewConfig[props.componentKind];
  if (!config) {
    return null;
  }

  const components = props.components;

  const normalizedFilter = filter.trim().toLowerCase();
  const filteredComponents = components.filter((component) =>
    component.identifier.toLowerCase().includes(normalizedFilter),
  );

  return (
    <div className="min-h-screen mr-4">
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
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredComponents.map((component) => (
                        <FormComponentTableRow
                          key={component.identifier}
                          component={component}
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
