import React, { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '../../lib/utils';

interface RawJsonBlockProps {
  readonly label: string;
  readonly columnName: string;
  readonly json?: string | null;
}

const RawJsonBlock = ({ label, columnName, json }: RawJsonBlockProps) => (
  <div>
    <div className="mb-1 flex flex-wrap items-baseline gap-2">
      <span className="font-semibold xp-text-default">{label}</span>
      <code className="xp-code-chip text-xs">{columnName}</code>
    </div>
    {json ? (
      <pre className="m-0 max-h-[32rem] overflow-auto rounded bg-slate-900 p-4 font-mono text-xs text-slate-100">
        {json}
      </pre>
    ) : (
      <p className="m-0 text-sm xp-empty-text">No data</p>
    )}
  </div>
);

interface RawJsonPanelProps {
  readonly templateLabel: string;
  readonly widgetsJson?: string | null;
  readonly templateConfigurationJson?: string | null;
}

export const RawJsonPanel = ({
  templateLabel,
  widgetsJson,
  templateConfigurationJson,
}: RawJsonPanelProps) => {
  const [expanded, setExpanded] = useState(false);

  return (
    <div className="rounded-lg border border-[#dfdfdf]">
      <button
        type="button"
        onClick={() => setExpanded(!expanded)}
        aria-expanded={expanded}
        className="flex w-full items-center gap-2 bg-transparent px-3 py-2 text-left font-semibold xp-text-default"
      >
        <ChevronDown
          size={16}
          className={cn(
            'xp-icon-muted transition-transform',
            !expanded && '-rotate-90',
          )}
        />
        Raw JSON
      </button>
      {expanded && (
        <div className="space-y-4 border-t border-[#dfdfdf] p-3">
          <RawJsonBlock
            label="Widgets (editable areas)"
            columnName="ContentItemCommonDataVisualBuilderWidgets"
            json={widgetsJson}
          />
          <RawJsonBlock
            label={`${templateLabel} configuration`}
            columnName="ContentItemCommonDataVisualBuilderTemplateConfiguration"
            json={templateConfigurationJson}
          />
        </div>
      )}
    </div>
  );
};
