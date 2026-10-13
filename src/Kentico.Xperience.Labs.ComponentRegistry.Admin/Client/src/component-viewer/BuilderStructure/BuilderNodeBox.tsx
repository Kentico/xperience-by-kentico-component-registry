import React, { useState } from 'react';
import { ChevronDown, TriangleAlert } from 'lucide-react';
import {
  BuilderDisplayMode,
  BuilderLabels,
  BuilderNodeDto,
  BuilderNodeKind,
} from './types';
import { ComponentIcon } from '../ui/component-icon';
import { PropertyList } from './PropertyList';
import { cn } from '../../lib/utils';

const kindAccentClasses: Record<BuilderNodeKind, string> = {
  root: 'border-l-slate-500',
  template: 'border-l-violet-500',
  editableArea: 'border-l-sky-500',
  section: 'border-l-emerald-500',
  zone: 'border-l-teal-400',
  widget: 'border-l-amber-500',
  variant: 'border-l-rose-400',
};

const getKindLabel = (kind: BuilderNodeKind, labels: BuilderLabels) =>
  ({
    root: labels.rootLabel,
    template: labels.templateLabel,
    editableArea: 'Editable area',
    section: 'Section',
    zone: 'Widget zone',
    widget: 'Widget',
    variant: 'Variant',
  })[kind];

const getTitle = (node: BuilderNodeDto, index: number, kindLabel: string) => {
  switch (node.kind) {
    case 'template':
    case 'section':
    case 'widget':
      return node.componentName ?? node.componentIdentifier ?? 'Unknown';
    case 'variant':
      // The first variant of a widget is the original, unnamed one
      return node.name ?? (index === 0 ? 'Original' : 'Unnamed variant');
    case 'zone':
      return node.name ?? `Zone ${index + 1}`;
    default:
      return node.name ?? kindLabel;
  }
};

const NotRegisteredTag = () => (
  <span
    className="inline-flex items-center gap-1 rounded-full border border-[#ffd9ae] bg-[#fff1e4] px-2 py-0.5 text-xs font-semibold text-[#9e6200]"
    title="No component with this identifier is registered in the application"
  >
    <TriangleAlert size={12} />
    Not registered
  </span>
);

interface BuilderNodeBoxProps {
  readonly node: BuilderNodeDto;
  readonly labels: BuilderLabels;
  readonly mode: BuilderDisplayMode;
  readonly index?: number;
}

// A widget with only its original variant shows the variant's data inline to save space
const collapseSingleVariant = (node: BuilderNodeDto): BuilderNodeDto => {
  if (
    node.kind !== 'widget' ||
    node.children.length !== 1 ||
    node.children[0].kind !== 'variant' ||
    node.children[0].conditionType
  ) {
    return node;
  }

  const [variant] = node.children;
  return {
    ...node,
    properties: [...node.properties, ...variant.properties],
    conditionParameters: [
      ...node.conditionParameters,
      ...variant.conditionParameters,
    ],
    children: variant.children,
  };
};

export const BuilderNodeBox = ({
  node: sourceNode,
  labels,
  mode,
  index = 0,
}: BuilderNodeBoxProps) => {
  const [expanded, setExpanded] = useState(true);
  const node = collapseSingleVariant(sourceNode);
  const kindLabel = getKindLabel(node.kind, labels);
  const showsComponent = node.componentIdentifier && node.kind !== 'variant';
  const detailed = mode === 'detailed';
  const hasBody =
    node.children.length > 0 ||
    (detailed &&
      (node.properties.length > 0 ||
        node.conditionParameters.length > 0 ||
        Boolean(node.conditionType) ||
        Boolean(node.configurationId)));

  return (
    <div
      className={cn(
        'border border-l-4 bg-white',
        detailed ? 'rounded-lg' : 'rounded-md',
        node.isRegistered === false ? 'border-[#ffd9ae]' : 'border-[#dfdfdf]',
        kindAccentClasses[node.kind],
      )}
    >
      <div
        className={cn(
          'flex flex-wrap items-center',
          detailed ? 'gap-2 px-3 py-2' : 'gap-1.5 px-2 py-0.5 text-sm',
        )}
      >
        <button
          type="button"
          onClick={() => setExpanded(!expanded)}
          disabled={!hasBody}
          aria-expanded={expanded}
          aria-label={expanded ? 'Collapse' : 'Expand'}
          className="xp-icon-button disabled:invisible"
        >
          <ChevronDown
            size={16}
            className={cn(
              'xp-icon-muted transition-transform',
              !expanded && '-rotate-90',
            )}
          />
        </button>
        <span className="text-xs font-semibold uppercase tracking-wide xp-text-muted">
          {kindLabel}
        </span>
        {node.iconClass && <ComponentIcon iconClass={node.iconClass} />}
        <span className="font-semibold xp-text-default">
          {getTitle(node, index, kindLabel)}
        </span>
        {showsComponent && (
          <code className="xp-code-chip text-xs">
            {node.componentIdentifier}
          </code>
        )}
        {node.isRegistered === false && <NotRegisteredTag />}
        {node.conditionType && <span className="xp-tag">Personalized</span>}
        {detailed && node.instanceId && (
          <span
            className="ml-auto font-mono text-xs xp-text-muted"
            title="Instance identifier"
          >
            {node.instanceId}
          </span>
        )}
      </div>
      {expanded && hasBody && (
        <div
          className={cn(
            'border-t border-[#dfdfdf]',
            detailed ? 'space-y-3 px-3 py-3' : 'space-y-1 p-1.5',
          )}
        >
          {detailed && node.configurationId && (
            <PropertyList
              title="Template configuration"
              properties={[
                {
                  name: 'configurationIdentifier',
                  value: node.configurationId,
                },
              ]}
            />
          )}
          {detailed && node.conditionType && (
            <PropertyList
              title="Personalization"
              properties={[
                { name: 'conditionType', value: node.conditionType },
              ]}
            />
          )}
          {detailed && node.conditionParameters.length > 0 && (
            <PropertyList
              title="Condition parameters"
              properties={node.conditionParameters}
            />
          )}
          {detailed && node.properties.length > 0 && (
            <PropertyList title="Properties" properties={node.properties} />
          )}
          {node.children.length > 0 && (
            <div className={detailed ? 'space-y-2' : 'space-y-1'}>
              {node.children.map((child, childIndex) => (
                <BuilderNodeBox
                  key={child.instanceId ?? `${child.kind}-${childIndex}`}
                  node={child}
                  labels={labels}
                  mode={mode}
                  index={childIndex}
                />
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};
