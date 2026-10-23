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
  AutomationProcessUsageDto,
  AutomationStepDto,
  AutomationStepUsageDetailDto,
  AutomationTriggerDto,
  UsageStats,
} from './ComponentDetails';

type AutomationStepKind = 'trigger' | 'action' | 'condition';

interface AutomationBuilderComponentViewerClientProperties {
  steps: (AutomationStepDto | AutomationTriggerDto)[];
  stepKind: AutomationStepKind;
  canViewUsages: boolean;
}

const viewConfig: Record<
  AutomationStepKind,
  {
    title: string;
    description: string;
    emptyText: string;
    usageLabel: string;
    usageItemLabel: string;
  }
> = {
  trigger: {
    title: 'Automation Triggers',
    description: 'Custom triggers that start automation processes',
    emptyText: 'No custom automation triggers registered',
    usageLabel: 'Triggers',
    usageItemLabel: 'Trigger',
  },
  action: {
    title: 'Automation Actions',
    description: 'Custom actions executed as automation process steps',
    emptyText: 'No custom automation actions registered',
    usageLabel: 'Steps',
    usageItemLabel: 'Step',
  },
  condition: {
    title: 'Automation Conditions',
    description: 'Custom conditions that branch automation processes',
    emptyText: 'No custom automation conditions registered',
    usageLabel: 'Steps',
    usageItemLabel: 'Step',
  },
};

const DetailRow: React.FC<{
  label: string;
  value?: string;
  mono?: boolean;
}> = ({ label, value, mono }) =>
  value ? (
    <div className="grid gap-1 sm:grid-cols-[11rem_1fr] sm:gap-4 sm:items-start">
      <dt className="font-medium xp-text-muted">{label}</dt>
      <dd
        className={`xp-text-muted ${mono ? 'font-mono text-xs break-all' : 'break-words'}`}
      >
        {value}
      </dd>
    </div>
  ) : null;

const formatDate = (value: string) => new Date(value).toLocaleString();

const ProcessUsageItem: React.FC<{
  process: AutomationProcessUsageDto;
  usageItemLabel: string;
}> = ({ process, usageItemLabel }) => (
  <div className="p-3 bg-[#fbfbfb] rounded border border-[#dfdfdf] text-xs">
    <div className="flex items-center justify-between gap-3">
      <div className="min-w-0 flex-1">
        <div className="font-medium xp-text-default mb-1 flex items-center gap-2">
          {process.processDisplayName}
          <span
            className={`px-2 py-0.5 rounded text-[10px] font-semibold ${
              process.isEnabled
                ? 'bg-[#e6f4ea] text-[#1e7d32]'
                : 'bg-[#f0f0f0] text-[#6b6b6b]'
            }`}
          >
            {process.isEnabled ? 'Enabled' : 'Disabled'}
          </span>
        </div>
        <div className="font-mono xp-text-muted text-xs">
          {process.processName}
        </div>
        {process.lastModified && (
          <div className="xp-text-muted text-xs mt-1">
            Process updated: {formatDate(process.lastModified)}
          </div>
        )}
      </div>
      {process.adminPath ? (
        <a
          href={process.adminPath}
          title="Open automation process builder"
          aria-label="Open automation process builder"
          className="text-[#3d5dff] hover:text-[#003ddc] flex-shrink-0 mr-2"
        >
          <Link size={20} />
        </a>
      ) : null}
    </div>

    <ul className="mt-3 space-y-2">
      {process.usages.map((usage) => (
        <li
          key={usage.objectID}
          className="p-2 bg-white rounded border border-[#dfdfdf]"
        >
          <div className="font-medium xp-text-default">{usage.displayName}</div>
          {usage.name && (
            <div className="font-mono xp-text-muted">{usage.name}</div>
          )}
          {usage.lastModified && (
            <div className="xp-text-muted mt-1">
              {usageItemLabel} updated: {formatDate(usage.lastModified)}
            </div>
          )}
          {usage.parameters.length > 0 && (
            <dl className="mt-2 grid gap-x-4 gap-y-1 sm:grid-cols-[11rem_1fr]">
              {usage.parameters.map((parameter) => (
                <React.Fragment key={parameter.name}>
                  <dt className="xp-text-muted font-medium">
                    {parameter.name}
                  </dt>
                  <dd className="xp-text-muted font-mono break-all whitespace-pre-wrap">
                    {parameter.value}
                  </dd>
                </React.Fragment>
              ))}
            </dl>
          )}
        </li>
      ))}
    </ul>
  </div>
);

const AutomationStepTableRow: React.FC<{
  step: AutomationStepDto | AutomationTriggerDto;
  usageLabel: string;
  usageItemLabel: string;
  canViewUsages: boolean;
}> = ({ step, usageLabel, usageItemLabel, canViewUsages }) => {
  const [expanded, setExpanded] = useState(false);
  const [usageData, setUsageData] =
    useState<AutomationStepUsageDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { execute: getAutomationStepUsage } = usePageCommand<
    AutomationStepUsageDetailDto,
    { componentIdentifier: string }
  >('GetAutomationStepUsage', {
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
        await getAutomationStepUsage({ componentIdentifier: step.identifier });
      } catch {
        setIsLoading(false);
      }
    }
    setExpanded(!expanded);
  };

  const trigger = step as AutomationTriggerDto;
  const hasStepInformation = Boolean(
    step.propertiesTypeName ||
    trigger.triggerDataTypeName ||
    trigger.objectType,
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
                ? 'Permission required to view automation step usages'
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
          <code className="xp-code-chip">{step.identifier}</code>
        </TableCell>
        <IconTableCell iconClass={step.iconName} />
        <TableCell className="font-semibold xp-text-default">
          {step.name}
        </TableCell>
        <TableCell className="xp-text-muted max-w-md">
          {step.description || (
            <span className="xp-muted-dash italic">No description</span>
          )}
        </TableCell>
        <TableCell>
          {step.implementationTypeName ? (
            <div
              className="max-w-xs overflow-x-auto overflow-y-hidden"
              title={step.implementationTypeName}
            >
              <code className="xp-code-chip whitespace-nowrap">
                {step.implementationTypeName}
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

                {hasStepInformation && (
                  <div className="mb-6 pb-6 border-b">
                    <dl className="text-sm space-y-3">
                      <DetailRow
                        label="Properties Type"
                        value={step.propertiesTypeName}
                        mono
                      />
                      <DetailRow
                        label="Trigger Data Type"
                        value={trigger.triggerDataTypeName}
                        mono
                      />
                      <DetailRow
                        label="Object Type"
                        value={trigger.objectType}
                        mono
                      />
                    </dl>
                  </div>
                )}

                <div>
                  <h5 className="text-sm font-medium xp-text-muted mb-4">
                    Step Usage
                  </h5>
                  {usageData ? (
                    <div className="space-y-3">
                      <UsageStats
                        stats={[
                          {
                            label: 'Automation Processes',
                            value: usageData.totalProcessesUsing,
                          },
                          { label: usageLabel, value: usageData.totalUsages },
                        ]}
                      />

                      {usageData.processes.length > 0 ? (
                        <div className="mt-4">
                          <div className="text-xs font-medium xp-text-muted mb-2">
                            Automation processes:
                          </div>
                          <div className="space-y-2 max-h-96 overflow-y-auto pr-3">
                            {usageData.processes.map((process) => (
                              <ProcessUsageItem
                                key={process.processID}
                                process={process}
                                usageItemLabel={usageItemLabel}
                              />
                            ))}
                          </div>
                        </div>
                      ) : (
                        <Callout type="warning">
                          No automation processes use this item
                        </Callout>
                      )}
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

export const AutomationBuilderComponentViewerTemplate = (
  props: AutomationBuilderComponentViewerClientProperties,
) => {
  const [filter, setFilter] = useState('');

  const config = viewConfig[props.stepKind];
  if (!config) {
    return null;
  }

  const steps = props.steps;

  const normalizedFilter = filter.trim().toLowerCase();
  const filteredSteps = steps.filter((step) =>
    step.identifier.toLowerCase().includes(normalizedFilter),
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
            {steps.length > 0 ? (
              <>
                <div className="mb-4">
                  <Input
                    type="text"
                    placeholder="Filter by identifier..."
                    value={filter}
                    onChange={(e) => setFilter(e.target.value)}
                  />
                </div>
                {filteredSteps.length > 0 ? (
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
                          Implementation Type
                        </TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {filteredSteps.map((step) => (
                        <AutomationStepTableRow
                          key={step.identifier}
                          step={step}
                          usageLabel={config.usageLabel}
                          usageItemLabel={config.usageItemLabel}
                          canViewUsages={props.canViewUsages}
                        />
                      ))}
                    </TableBody>
                  </Table>
                ) : (
                  <div className="text-center py-8 xp-empty-text">
                    <p>No items match this identifier filter</p>
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
