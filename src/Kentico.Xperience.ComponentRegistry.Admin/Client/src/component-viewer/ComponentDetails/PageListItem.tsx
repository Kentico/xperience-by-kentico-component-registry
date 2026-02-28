import React, { useState } from 'react';
import { ChevronDown } from 'lucide-react';
import { LanguageVariantList } from './LanguageVariantList';
import { PageUsageDto } from './types';

interface PageListItemProps {
  page: PageUsageDto;
  inspectedComponentIdentifier: string;
  inspectedComponentType: string;
  inspectedComponentTypeName?: string;
}

export const PageListItem: React.FC<PageListItemProps> = ({
  page,
  inspectedComponentIdentifier,
  inspectedComponentType,
  inspectedComponentTypeName,
}) => {
  const [variantsExpanded, setVariantsExpanded] = useState(false);

  const modifiedDate = page.modifiedAt
    ? new Date(page.modifiedAt).toLocaleDateString()
    : new Date(page.createdAt).toLocaleDateString();

  return (
    <div className="border border-slate-300 rounded-lg overflow-hidden bg-white mb-3">
      <button
        onClick={() => setVariantsExpanded(!variantsExpanded)}
        className="w-full px-4 py-3 flex items-center justify-between hover:bg-slate-50 transition-colors text-left"
      >
        <div className="flex-1">
          <p className="text-sm font-medium text-slate-900">{page.pageName}</p>
          <p className="text-xs text-slate-600">
            {page.channelDisplayName} • {page.pagePath} • {page.variants.length}{' '}
            variant
            {page.variants.length !== 1 ? 's' : ''} • Modified: {modifiedDate}
          </p>
        </div>

        <ChevronDown
          size={20}
          className={`text-slate-600 transition-transform flex-shrink-0 ml-2 ${
            variantsExpanded ? '' : '-rotate-90'
          }`}
        />
      </button>

      {variantsExpanded && (
        <div className="px-4 py-3 bg-slate-50 border-t border-slate-300">
          <p className="text-xs font-semibold text-slate-700 mb-3">
            Language Variants:
          </p>
          <LanguageVariantList
            variants={page.variants}
            inspectedComponentIdentifier={inspectedComponentIdentifier}
            inspectedComponentType={inspectedComponentType}
            inspectedComponentTypeName={inspectedComponentTypeName}
          />
        </div>
      )}
    </div>
  );
};
