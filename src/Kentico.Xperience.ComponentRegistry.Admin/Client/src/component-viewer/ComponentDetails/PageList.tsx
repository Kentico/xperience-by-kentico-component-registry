import React, { useMemo, useState } from 'react';
import { ChevronDown, Search } from 'lucide-react';
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
  const [selectedContentType, setSelectedContentType] = useState('');

  const contentTypeOptions = useMemo(
    () =>
      [...new Set(pages.map((page) => page.contentTypeDisplayName).filter(Boolean))]
        .sort((a, b) => a.localeCompare(b)),
    [pages],
  );

  const filteredPages = useMemo(() => {
    const normalizedSearch = searchTerm.trim().toLowerCase();

    return pages.filter((page) => {
      const matchesSearch = !normalizedSearch ||
        page.pageName.toLowerCase().includes(normalizedSearch);
      const matchesContentType = !selectedContentType ||
        page.contentTypeDisplayName === selectedContentType;

      return matchesSearch && matchesContentType;
    });
  }, [pages, searchTerm, selectedContentType]);

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
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
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
          <div className="xp-input-wrapper relative min-w-48 sm:w-64">
            <select
              value={selectedContentType}
              onChange={(e) => setSelectedContentType(e.target.value)}
              className="xp-input appearance-none pr-10 text-sm"
              aria-label="Filter by content type"
            >
              <option value="">All content types</option>
              {contentTypeOptions.map((contentType) => (
                <option key={contentType} value={contentType}>
                  {contentType}
                </option>
              ))}
            </select>
            <ChevronDown
              size={16}
              className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 xp-icon-muted"
            />
          </div>
        </div>
      </div>

      <h3 className="text-sm font-semibold text-slate-900 mb-3">
        Pages Using Component ({filteredPages.length}
        {filteredPages.length !== pages.length && ` of ${pages.length}`})
      </h3>
      <div className="space-y-2">
        {filteredPages.length === 0 ? (
          <Callout type="info">
            No pages match the selected filters.
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
