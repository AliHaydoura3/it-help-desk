import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";

interface CommunicationPaginationProps {
  noun: string;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  disabled?: boolean;
  onPageChange: (pageNumber: number) => void;
}

export function CommunicationPagination({
  noun,
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  disabled = false,
  onPageChange,
}: CommunicationPaginationProps) {
  const lastPage = Math.max(1, totalPages);
  const currentPage = Math.min(Math.max(1, pageNumber), lastPage);
  const first = totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1;
  const last = Math.min(currentPage * pageSize, totalCount);

  return (
    <div className="flex flex-col gap-3 border-t px-5 py-3 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-xs text-muted-foreground">
        Showing {first}–{last} of {totalCount} {noun} · Page {currentPage} of {lastPage}
      </p>
      <nav aria-label={`${noun} pagination`} className="flex items-center gap-1">
        <Button
          aria-label="Previous page"
          disabled={disabled || currentPage === 1}
          onClick={() => onPageChange(currentPage - 1)}
          size="icon-sm"
          variant="outline"
        >
          <ChevronLeft />
        </Button>
        {getVisiblePages(currentPage, lastPage).map((item) =>
          typeof item === "number" ? (
            <Button
              aria-current={item === currentPage ? "page" : undefined}
              aria-label={`Page ${item}`}
              disabled={disabled}
              key={item}
              onClick={() => onPageChange(item)}
              size="icon-sm"
              variant={item === currentPage ? "default" : "outline"}
            >
              {item}
            </Button>
          ) : (
            <span className="flex size-7 items-center justify-center text-xs text-muted-foreground" key={item}>…</span>
          ),
        )}
        <Button
          aria-label="Next page"
          disabled={disabled || currentPage === lastPage}
          onClick={() => onPageChange(currentPage + 1)}
          size="icon-sm"
          variant="outline"
        >
          <ChevronRight />
        </Button>
      </nav>
    </div>
  );
}

function getVisiblePages(currentPage: number, totalPages: number): Array<number | string> {
  if (totalPages <= 7) return Array.from({ length: totalPages }, (_, index) => index + 1);
  if (currentPage <= 4) return [1, 2, 3, 4, 5, "right-gap", totalPages];
  if (currentPage >= totalPages - 3) {
    return [1, "left-gap", totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
  }
  return [1, "left-gap", currentPage - 1, currentPage, currentPage + 1, "right-gap", totalPages];
}
