import React from 'react';

interface JsonViewerProps {
  json: string;
  highlightComponentIdentifier?: string;
  highlightComponentTypeName?: string;
  enableComponentHighlight?: boolean;
}

const normalizeTypeValue = (value: string) =>
  value
    .trim()
    .toLowerCase()
    .replace(/^global::/, '');

const getComparableTypeValues = (value: string) => {
  const normalized = normalizeTypeValue(value);
  const withoutAssembly = normalized.split(',')[0].trim();
  return Array.from(new Set([normalized, withoutAssembly]));
};

const typeValuesMatch = (valueA: string, valueB: string) => {
  const comparableA = getComparableTypeValues(valueA);
  const comparableB = getComparableTypeValues(valueB);

  return comparableA.some((left) =>
    comparableB.some(
      (right) =>
        left === right ||
        left.endsWith(`.${right}`) ||
        right.endsWith(`.${left}`),
    ),
  );
};

export const JsonViewer: React.FC<JsonViewerProps> = ({
  json,
  highlightComponentIdentifier,
  highlightComponentTypeName,
  enableComponentHighlight = false,
}) => {
  let parsed: unknown;

  try {
    parsed = JSON.parse(json);
  } catch {
    return (
      <div className="p-2 bg-red-50 border border-red-200 rounded text-red-700 text-sm">
        Invalid JSON configuration
      </div>
    );
  }

  const highlightTargets = enableComponentHighlight
    ? [highlightComponentIdentifier, highlightComponentTypeName].filter(
        (value): value is string => Boolean(value?.trim()),
      )
    : [];

  const indentation = (depth: number) => '  '.repeat(depth);

  const isTargetComponentObject = (value: unknown) => {
    if (
      highlightTargets.length === 0 ||
      !value ||
      typeof value !== 'object' ||
      Array.isArray(value)
    ) {
      return false;
    }

    const typeValue = (value as Record<string, unknown>).type;
    return (
      typeof typeValue === 'string' &&
      highlightTargets.some((target) => typeValuesMatch(typeValue, target))
    );
  };

  const renderJsonValue = (
    value: unknown,
    depth: number,
    isInHighlightedBlock: boolean,
  ): React.ReactNode => {
    if (Array.isArray(value)) {
      if (value.length === 0) {
        return '[]';
      }

      return (
        <>
          {'['}
          {'\n'}
          {value.map((item, index) => (
            <React.Fragment key={index}>
              {indentation(depth + 1)}
              {renderJsonValue(item, depth + 1, isInHighlightedBlock)}
              {index < value.length - 1 ? ',' : ''}
              {'\n'}
            </React.Fragment>
          ))}
          {indentation(depth)}
          {']'}
        </>
      );
    }

    if (value && typeof value === 'object') {
      const entries = Object.entries(value as Record<string, unknown>);
      if (entries.length === 0) {
        return '{}';
      }

      const isTargetObject = isTargetComponentObject(value);
      const shouldHighlightObject = isTargetObject && !isInHighlightedBlock;
      const isInBlock = isInHighlightedBlock || shouldHighlightObject;

      const objectContent = (
        <>
          {'{'}
          {'\n'}
          {entries.map(([key, entryValue], index) => (
            <React.Fragment key={key}>
              {indentation(depth + 1)}
              {JSON.stringify(key)}:{' '}
              {renderJsonValue(entryValue, depth + 1, isInBlock)}
              {index < entries.length - 1 ? ',' : ''}
              {'\n'}
            </React.Fragment>
          ))}
          {indentation(depth)}
          {'}'}
        </>
      );

      return shouldHighlightObject ? (
        <span className="rounded bg-amber-300/20 ring-1 ring-amber-200/40">
          {objectContent}
        </span>
      ) : (
        objectContent
      );
    }

    return JSON.stringify(value);
  };

  return (
    <div className="p-4 bg-slate-900 text-slate-100 rounded overflow-auto text-xs font-mono whitespace-pre">
      {renderJsonValue(parsed, 0, false)}
    </div>
  );
};
