import { ChevronLeft, ChevronRight } from "lucide-react";

import { Button } from "@/components/ui/button";

interface AttachmentPaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  disabled: boolean;
  onPageChange: (pageNumber: number) => void;
}

export function AttachmentPagination({
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  disabled,
  onPageChange,
}: AttachmentPaginationProps) {
  if (totalCount === 0) return null;
  const lastPage = Math.max(1, totalPages);
  const firstItem = (pageNumber - 1) * pageSize + 1;
  const lastItem = Math.min(pageNumber * pageSize, totalCount);

  return (
    <div className="flex items-center justify-between border-t px-4 py-3 sm:px-6">
      <p className="text-xs text-muted-foreground">
        {firstItem}–{lastItem} of {totalCount} files
      </p>
      <div className="flex items-center gap-2">
        <span className="hidden text-xs text-muted-foreground sm:inline">Page {pageNumber} of {lastPage}</span>
        <Button aria-label="Previous attachment page" disabled={disabled || pageNumber <= 1} onClick={() => onPageChange(pageNumber - 1)} size="icon-sm" variant="outline"><ChevronLeft /></Button>
        <Button aria-label="Next attachment page" disabled={disabled || pageNumber >= lastPage} onClick={() => onPageChange(pageNumber + 1)} size="icon-sm" variant="outline"><ChevronRight /></Button>
      </div>
    </div>
  );
}
