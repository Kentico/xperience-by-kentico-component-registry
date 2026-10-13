import React, { useState } from 'react';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from './ui/card';
import { Callout } from './ui/callout';
import { SegmentedControl } from './ui/segmented-control';
import { BuilderNodeBox } from './BuilderStructure/BuilderNodeBox';
import { RawJsonPanel } from './BuilderStructure/RawJsonPanel';
import {
  BuilderDisplayMode,
  BuilderStructureClientProperties,
  VersionStatus,
  builderLabels,
} from './BuilderStructure/types';

const versionLabels: Record<VersionStatus, string> = {
  InitialDraft: 'Draft (never published)',
  Draft: 'Draft',
  Published: 'Published',
  Unpublished: 'Unpublished',
};

// Shared by the web page and email tabs
const displayModeStorageKey = 'componentRegistry.builderStructure.displayMode';

const displayModeOptions = [
  { value: 'simple', label: 'Simple' },
  { value: 'detailed', label: 'Detailed' },
] as const;

const readDisplayMode = (): BuilderDisplayMode => {
  try {
    return localStorage.getItem(displayModeStorageKey) === 'detailed'
      ? 'detailed'
      : 'simple';
  } catch {
    return 'simple';
  }
};

const storeDisplayMode = (mode: BuilderDisplayMode) => {
  try {
    localStorage.setItem(displayModeStorageKey, mode);
  } catch {
    // Storage can be unavailable (e.g. blocked by browser settings)
  }
};

const EmptyState = ({ text }: { text: string }) => (
  <div className="text-center py-12 xp-empty-text">
    <p className="text-lg">{text}</p>
  </div>
);

export const BuilderStructureTemplate = ({
  builder,
  languageName,
  structure,
}: BuilderStructureClientProperties) => {
  const labels = builderLabels[builder] ?? builderLabels.pageBuilder;
  const [mode, setMode] = useState<BuilderDisplayMode>(readDisplayMode);

  const changeMode = (newMode: BuilderDisplayMode) => {
    setMode(newMode);
    storeDisplayMode(newMode);
  };

  return (
    <div className="min-h-screen mr-4">
      <div className="w-full max-w-[110rem] mx-auto">
        <Card>
          <CardHeader className="flex-row flex-wrap items-start justify-between gap-x-4 gap-y-2">
            <div className="flex flex-col gap-1">
              <CardTitle className="text-2xl">Builder structure</CardTitle>
              <CardDescription className="text-base">
                {structure
                  ? `${labels.builderName} configuration of the latest version in ${structure.languageDisplayName} (${structure.languageName}). Version: ${versionLabels[structure.versionStatus] ?? structure.versionStatus}.`
                  : `${labels.builderName} configuration of the latest ${labels.subject} version`}
              </CardDescription>
            </div>
            {structure?.root && (
              <SegmentedControl
                ariaLabel="Display mode"
                options={displayModeOptions}
                value={mode}
                onChange={changeMode}
              />
            )}
          </CardHeader>
          <CardContent className="pt-6 space-y-4">
            {!structure ? (
              <EmptyState
                text={`The ${labels.subject} has no language variant in ${languageName}`}
              />
            ) : (
              <>
                {structure.differsFromPublished && (
                  <Callout type="info">
                    The latest version has unpublished {labels.builderName}{' '}
                    changes. The published version differs from what is shown
                    here.
                  </Callout>
                )}
                {structure.errors.length > 0 && (
                  <Callout type="warning">
                    <ul className="m-0 list-disc pl-5">
                      {structure.errors.map((error) => (
                        <li key={error}>{error}</li>
                      ))}
                    </ul>
                  </Callout>
                )}
                {structure.root ? (
                  // Remounting on mode change resets the per-box collapse state
                  <BuilderNodeBox
                    key={mode}
                    node={structure.root}
                    labels={labels}
                    mode={mode}
                  />
                ) : (
                  <EmptyState
                    text={`This ${labels.subject} has no ${labels.builderName} content`}
                  />
                )}
                {(structure.widgetsJson ||
                  structure.templateConfigurationJson) && (
                  <RawJsonPanel
                    templateLabel={labels.templateLabel}
                    widgetsJson={structure.widgetsJson}
                    templateConfigurationJson={
                      structure.templateConfigurationJson
                    }
                  />
                )}
              </>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
};
