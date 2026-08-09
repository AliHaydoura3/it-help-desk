import { ChevronLeft, ChevronRight } from "lucide-react";

import { Button } from "@/components/ui/button";

interface ReportingPaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  disabled?: boolean;
  onPageChange: (page: number) => void;
}

export function ReportingPagination({
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  disabled,
  onPageChange,
}: ReportingPaginationProps) {
  if (totalCount === 0) return null;
  const first = (pageNumber - 1) * pageSize + 1;
  const last = Math.min(pageNumber * pageSize, totalCount);

  return (
    <div className="flex flex-col gap-3 border-t px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-xs text-muted-foreground">
        Showing {first}–{last} of {totalCount} users
      </p>
      <div className="flex items-center gap-2">
        <span className="mr-1 text-xs text-muted-foreground">Page {pageNumber} of {Math.max(1, totalPages)}</span>
        <Button aria-label="Previous page" disabled={disabled || pageNumber <= 1} onClick={() => onPageChange(pageNumber - 1)} size="icon-sm" variant="outline"><ChevronLeft /></Button>
        <Button aria-label="Next page" disabled={disabled || pageNumber >= totalPages} onClick={() => onPageChange(pageNumber + 1)} size="icon-sm" variant="outline"><ChevronRight /></Button>
      </div>
    </div>
  );
}
