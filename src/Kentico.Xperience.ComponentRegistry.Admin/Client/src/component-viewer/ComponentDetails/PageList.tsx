import React, { useMemo, useState } from 'react';
import { Search } from 'lucide-react';
import { PageListItem } from './PageListItem';
import { PageUsageDto } from './types';

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
      <div className="p-4 bg-slate-50 rounded-lg border border-slate-200">
        <p className="text-sm text-slate-600">
          No pages are using this component.
        </p>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-4">
        <div className="relative">
          <Search
            size={16}
            className="absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400"
          />
          <input
            type="text"
            placeholder="Search pages by name..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-9 pr-3 py-2 text-sm text-slate-900 bg-white border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent placeholder-slate-500"
          />
        </div>
      </div>

      <h3 className="text-sm font-semibold text-slate-900 mb-3">
        Pages Using Component ({filteredPages.length}
        {filteredPages.length !== pages.length && ` of ${pages.length}`})
      </h3>
      <div className="space-y-2">
        {filteredPages.length === 0 ? (
          <p className="text-sm text-slate-600 p-4 bg-slate-50 rounded-lg border border-slate-200">
            No pages match &quot;{searchTerm}&quot;.
          </p>
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
