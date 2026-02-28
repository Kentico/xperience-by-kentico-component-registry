import React from 'react';
import { LanguageVariantItem } from './LanguageVariantItem';
import { EmailConfigurationVariantDto, PageVariantDto } from './types';

interface LanguageVariantListProps {
  variants: PageVariantDto[] | EmailConfigurationVariantDto[];
  inspectedComponentIdentifier: string;
  inspectedComponentType: string;
  inspectedComponentTypeName?: string;
}

export const LanguageVariantList: React.FC<LanguageVariantListProps> = ({
  variants,
  inspectedComponentIdentifier,
  inspectedComponentType,
  inspectedComponentTypeName,
}) => {
  if (variants.length === 0) {
    return (
      <p className="text-sm text-slate-500 py-2">No language variants found</p>
    );
  }

  return (
    <div className="space-y-3">
      {variants.map((variant) => (
        <LanguageVariantItem
          key={variant.contentItemCommonDataId}
          variant={variant}
          inspectedComponentIdentifier={inspectedComponentIdentifier}
          inspectedComponentType={inspectedComponentType}
          inspectedComponentTypeName={inspectedComponentTypeName}
        />
      ))}
    </div>
  );
};
