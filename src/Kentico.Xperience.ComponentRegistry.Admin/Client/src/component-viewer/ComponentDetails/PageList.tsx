import React, { useMemo, useState } from 'react';
import { Search } from 'lucide-react';
import { PageListItem } from './PageListItem';
import { PageUsageDto } from './types';
import { Input } from '../ui/input';
import { Callout } from '../ui/callout';

interface PageListProps {
  pages: PageUsageDto[];
  inspectedComponentIdentifier: string;
  inspectedComponentType: string;
  inspectedComponentTypeName?: string;
}

export const PageList: React.FC<PageListProps> = ({
  pages,
  inspectedComponentIdentifier,
  inspectedComponentType,
  inspectedComponentTypeName,
}) => {
  const [searchTerm, setSearchTerm] = useState('');

  const filteredPages = useMemo(() => {
    if (!searchTerm.trim()) {
      return pages;
    }
    return pages.filter((page) =>
      page.pageName.toLowerCase().includes(searchTerm.toLowerCase()),
    );
  }, [pages, searchTerm]);

  if (pages.length === 0) {
    return (
      <Callout type="info">
        <p>
          No pages are using this component.
        </p>
      </Callout>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <div className="relative">
          <Search
            size={16}
            className="absolute left-3 top-1/2 transform -translate-y-1/2 xp-muted-dash"
          />
          <Input
            type="text"
            placeholder="Search pages by name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-9 pr-3 text-sm"
          />
        </div>
      </div>

      <h3 className="text-sm font-semibold text-slate-900 mb-3">
        Pages Using Component ({filteredPages.length}
        {filteredPages.length !== pages.length && ` of ${pages.length}`})
      </h3>
      <div className="space-y-2">
        {filteredPages.length === 0 ? (
          <Callout type="info">
            No pages match &quot;{searchTerm}&quot;.
          </Callout>
        ) : (
          filteredPages.map((page) => (
            <PageListItem
              key={page.webPageItemId}
              page={page}
              inspectedComponentIdentifier={inspectedComponentIdentifier}
              inspectedComponentType={inspectedComponentType}
              inspectedComponentTypeName={inspectedComponentTypeName}
            />
          ))
        )}
      </div>
    </div>
  );
};
