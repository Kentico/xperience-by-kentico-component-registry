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
import { Callout } from './ui/callout';
import { Checkbox } from '@kentico/xperience-admin-components';
import { ChevronDown, Link, Loader } from 'lucide-react';
import {
  AdminFormComponentClassUsageDto,
  AdminFormComponentDto,
  AdminFormComponentFieldUsageDto,
  AdminFormComponentUsageDetailDto,
  UsageStats,
} from './ComponentDetails';

interface AdminFormComponentViewerClientProperties {
  components: AdminFormComponentDto[];
  canViewUsages: boolean;
}

const formatDate = (value: string) => new Date(value).toLocaleString();

type ClassKind = 'content' | 'schema' | 'object';

const classKinds: { kind: ClassKind; label: string }[] = [
  { kind: 'content', label: 'Content type' },
  { kind: 'schema', label: 'Reusable field schema' },
  { kind: 'object', label: 'Object type' },
];

const getClassKind = (usedClass: AdminFormComponentClassUsageDto): ClassKind =>
  usedClass.reusableFieldSchemaIdentifier
    ? 'schema'
    : usedClass.classType === 'Content'
      ? 'content'
      : 'object';

const FieldUsageList: React.FC<{
  fields: AdminFormComponentFieldUsageDto[];
}> = ({ fields }) =>
  fields.length > 0 ? (
    <ul className="mt-2 space-y-1">
      {fields.map((field) => (
        <li
          key={field.fieldGuid ?? field.fieldName}
          className="font-mono xp-text-default flex items-center gap-2"
        >
          <span>
            {field.fieldName}
            {field.fieldCaption && (
              <span className="xp-text-muted font-sans">
                {' '}
                — {field.fieldCaption}
              </span>
            )}
          </span>
        </li>
      ))}
    </ul>
  ) : null;

const AdminFormComponentTableRow: React.FC<{
  component: AdminFormComponentDto;
  canViewUsages: boolean;
}> = ({ component, canViewUsages }) => {
  const [expanded, setExpanded] = useState(false);
  const [classFilter, setClassFilter] = useState('');
  const [selectedKinds, setSelectedKinds] = useState<ClassKind[]>([]);
  const [usageData, setUsageData] =
    useState<AdminFormComponentUsageDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { execute: getComponentUsage } = usePageCommand<
    AdminFormComponentUsageDetailDto,
    { componentIdentifier: string }
  >('GetAdminFormComponentUsage', {
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

  const normalizedClassFilter = classFilter.trim().toLowerCase();
  const filteredClasses = (usageData?.classes ?? []).filter(
    (c) =>
      (selectedKinds.length === 0 || selectedKinds.includes(getClassKind(c))) &&
      (!normalizedClassFilter ||
        c.classDisplayName.toLowerCase().includes(normalizedClassFilter) ||
        c.className.toLowerCase().includes(normalizedClassFilter)),
  );

  const toggleKind = (kind: ClassKind) =>
    setSelectedKinds((current) =>
      current.includes(kind)
        ? current.filter((k) => k !== kind)
        : [...current, kind],
    );

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
        <TableCell className="font-semibold xp-text-default">
          {component.name}
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
          <TableCell colSpan={4} className="p-0">
            <div className="p-4 xp-panel-subtle border-t">
              <div className="bg-white p-4 rounded border border-[#dfdfdf]">
                <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
                  <div className="space-y-3">
                    <h4 className="font-semibold xp-text-default">Details</h4>
                    <h5 className="text-sm font-medium xp-text-muted">
                      Component Usage
                    </h5>
                    {usageData && (
                      <UsageStats
                        stats={[
                          {
                            label: 'Total Types',
                            value: usageData.totalClassesUsing,
                          },
                          {
                            label: 'Total Fields',
                            value: usageData.totalFieldsUsing,
                          },
                        ]}
                      />
                    )}
                    {usageData && usageData.classes.length > 0 && (
                      <>
                        <Input
                          placeholder="Filter by name or code name..."
                          value={classFilter}
                          onChange={(e) => setClassFilter(e.target.value)}
                        />
                        <div
                          className="flex flex-col gap-1"
                          role="group"
                          aria-label="Filter by type"
                        >
                          {classKinds.map(({ kind, label }) => {
                            const count = usageData.classes.filter(
                              (c) => getClassKind(c) === kind,
                            ).length;
                            return (
                              <Checkbox
                                key={kind}
                                label={`${label} (${count})`}
                                checked={selectedKinds.includes(kind)}
                                onChange={() => toggleKind(kind)}
                              />
                            );
                          })}
                        </div>
                      </>
                    )}
                    <p className="text-xs xp-text-muted">
                      Based on field editor assignments saved in class form
                      definitions and class UI forms. Components used only in C#
                      code are not detected.
                    </p>
                  </div>
                  <div className="md:col-span-4">
                    {usageData ? (
                      usageData.classes.length > 0 ? (
                        <div>
                          <div className="text-xs font-medium xp-text-muted mb-2">
                            Classes and fields:
                          </div>
                          <div className="space-y-2 max-h-96 overflow-y-auto pr-3">
                            {filteredClasses.length === 0 && (
                              <div className="xp-empty-text italic text-xs">
                                No classes match the filter
                              </div>
                            )}
                            {filteredClasses.map((usedClass) => (
                              <div
                                key={`${usedClass.classId}-${usedClass.reusableFieldSchemaIdentifier ?? ''}`}
                                className="p-3 bg-[#fbfbfb] rounded border border-[#dfdfdf] text-xs"
                              >
                                <div className="flex items-start justify-between gap-2">
                                  <div>
                                    <div className="font-medium xp-text-default mb-1">
                                      {usedClass.classDisplayName}
                                    </div>
                                    <div className="font-mono xp-text-muted text-xs">
                                      {usedClass.className}
                                      {usedClass.reusableFieldSchemaIdentifier
                                        ? ' (Reusable field schema)'
                                        : usedClass.classType
                                          ? ` (${usedClass.classType})`
                                          : ''}
                                    </div>
                                    {usedClass.classLastModified && (
                                      <div className="xp-text-muted text-xs mt-1">
                                        Class updated:{' '}
                                        {formatDate(
                                          usedClass.classLastModified,
                                        )}
                                      </div>
                                    )}
                                  </div>
                                  {usedClass.adminPath ? (
                                    <a
                                      href={usedClass.adminPath}
                                      title={
                                        usedClass.reusableFieldSchemaIdentifier
                                          ? 'Open reusable field schema fields'
                                          : 'Open content type fields'
                                      }
                                      aria-label={
                                        usedClass.reusableFieldSchemaIdentifier
                                          ? 'Open reusable field schema fields'
                                          : 'Open content type fields'
                                      }
                                      className="text-[#3d5dff] hover:text-[#003ddc] flex-shrink-0 mr-2"
                                    >
                                      <Link size={20} />
                                    </a>
                                  ) : null}
                                </div>
                                <FieldUsageList fields={usedClass.fields} />
                                {usedClass.uiForms.map((uiForm) => (
                                  <div
                                    key={uiForm.formId}
                                    className="mt-2 pl-3 border-l-2 border-[#dfdfdf]"
                                  >
                                    <div className="flex items-center gap-2 xp-text-default">
                                      <span>
                                        UI form: {uiForm.formDisplayName}{' '}
                                        <span className="font-mono xp-text-muted">
                                          ({uiForm.formName})
                                        </span>
                                      </span>
                                      {uiForm.adminPath ? (
                                        <a
                                          href={uiForm.adminPath}
                                          title="Open UI form fields"
                                          aria-label="Open UI form fields"
                                          className="text-[#3d5dff] hover:text-[#003ddc] flex-shrink-0"
                                        >
                                          <Link size={14} />
                                        </a>
                                      ) : null}
                                    </div>
                                    <FieldUsageList fields={uiForm.fields} />
                                  </div>
                                ))}
                              </div>
                            ))}
                          </div>
                        </div>
                      ) : (
                        <Callout type="warning">
                          No saved class field uses this component
                        </Callout>
                      )
                    ) : (
                      <div className="xp-empty-text italic">
                        Loading usage information...
                      </div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </TableCell>
        </TableRow>
      )}
    </>
  );
};

export const AdminFormComponentViewerTemplate = (
  props: AdminFormComponentViewerClientProperties,
) => {
  const [filter, setFilter] = useState('');

  const normalizedFilter = filter.trim().toLowerCase();
  const filteredComponents = props.components.filter((component) =>
    component.identifier.toLowerCase().includes(normalizedFilter),
  );

  return (
    <div className="min-h-screen mr-4">
      <div className="w-full max-w-[110rem] mx-auto">
        <Card>
          <CardHeader>
            <CardTitle className="text-2xl">Form Components</CardTitle>
            <CardDescription className="text-base">
              Administration UI form components assignable to fields in the
              field editor. Not Form Builder components.
            </CardDescription>
          </CardHeader>
          <CardContent className="pt-6">
            {props.components.length > 0 ? (
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
                        <TableHead className="font-semibold">Name</TableHead>
                        <TableHead className="font-semibold">
                          Component Type
                        </TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredComponents.map((component) => (
                        <AdminFormComponentTableRow
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
                <p className="text-lg">
                  No Admin UI form components registered
                </p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};
