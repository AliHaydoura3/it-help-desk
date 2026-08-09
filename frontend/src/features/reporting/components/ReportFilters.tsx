import { CalendarDays, RotateCcw } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

interface ReportFiltersProps {
  from: string;
  to: string;
  onFromChange: (value: string) => void;
  onToChange: (value: string) => void;
  onReset: () => void;
}

export function ReportFilters({
  from,
  to,
  onFromChange,
  onToChange,
  onReset,
}: ReportFiltersProps) {
  return (
    <div className="flex flex-col gap-3 rounded-xl border bg-card p-3 sm:flex-row sm:items-end">
      <div className="hidden size-9 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground sm:flex">
        <CalendarDays className="size-4" />
      </div>
      <div className="grid flex-1 gap-3 sm:grid-cols-2">
        <div className="space-y-1.5">
          <Label className="text-xs" htmlFor="report-from">From</Label>
          <Input id="report-from" max={to || undefined} onChange={(event) => onFromChange(event.target.value)} type="date" value={from} />
        </div>
        <div className="space-y-1.5">
          <Label className="text-xs" htmlFor="report-to">To</Label>
          <Input id="report-to" min={from || undefined} onChange={(event) => onToChange(event.target.value)} type="date" value={to} />
        </div>
      </div>
      <Button aria-label="Reset reporting dates" onClick={onReset} size="icon-lg" title="Reset reporting dates" variant="outline">
        <RotateCcw />
      </Button>
    </div>
  );
}
