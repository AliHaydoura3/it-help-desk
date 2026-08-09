import { cn } from "@/lib/utils";

export interface BarChartItem {
  label: string;
  value: number;
  displayValue?: string;
  tone?: "dark" | "mid" | "light" | "danger";
}

const TONES = {
  dark: "bg-foreground",
  mid: "bg-muted-foreground",
  light: "bg-foreground/35",
  danger: "bg-destructive/75",
} as const;

export function HorizontalBarChart({ items }: { items: BarChartItem[] }) {
  const maximum = Math.max(1, ...items.map((item) => item.value));

  return (
    <div className="space-y-4" role="img" aria-label="Horizontal bar chart">
      {items.map((item) => (
        <div key={item.label}>
          <div className="mb-1.5 flex items-center justify-between gap-4 text-xs">
            <span className="truncate text-muted-foreground">{item.label}</span>
            <span className="font-semibold tabular-nums">{item.displayValue ?? item.value.toLocaleString()}</span>
          </div>
          <div className="h-2.5 overflow-hidden rounded-full bg-muted">
            <div
              className={cn("h-full min-w-px rounded-full transition-[width] duration-500", TONES[item.tone ?? "dark"])}
              style={{ width: `${item.value === 0 ? 0 : Math.max(3, item.value / maximum * 100)}%` }}
            />
          </div>
        </div>
      ))}
    </div>
  );
}
