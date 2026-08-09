import type { MonthlyTicketMetric } from "../types/reporting";

const WIDTH = 800;
const HEIGHT = 260;
const LEFT = 36;
const TOP = 18;
const BOTTOM = 42;

export function MonthlyTrendChart({ months }: { months: MonthlyTicketMetric[] }) {
  if (months.length === 0) {
    return <div className="py-16 text-center text-sm text-muted-foreground">No monthly data available.</div>;
  }

  const plotWidth = WIDTH - LEFT - 18;
  const plotHeight = HEIGHT - TOP - BOTTOM;
  const maximum = Math.max(1, ...months.flatMap((month) => [month.createdTickets, month.resolvedTickets]));
  const x = (index: number) => LEFT + (months.length === 1 ? plotWidth / 2 : index * plotWidth / (months.length - 1));
  const y = (value: number) => TOP + plotHeight - value / maximum * plotHeight;
  const created = months.map((month, index) => `${x(index)},${y(month.createdTickets)}`).join(" ");
  const resolved = months.map((month, index) => `${x(index)},${y(month.resolvedTickets)}`).join(" ");
  const labelStep = Math.max(1, Math.ceil(months.length / 6));

  return (
    <div>
      <div className="mb-3 flex flex-wrap gap-4 text-xs text-muted-foreground">
        <span className="inline-flex items-center gap-2"><span className="size-2.5 rounded-full bg-foreground" /> Created</span>
        <span className="inline-flex items-center gap-2"><span className="size-2.5 rounded-full bg-emerald-600" /> Resolved</span>
      </div>
      <svg className="h-auto w-full" viewBox={`0 0 ${WIDTH} ${HEIGHT}`} role="img" aria-label="Created and resolved ticket trend">
        {[0, 0.25, 0.5, 0.75, 1].map((ratio) => {
          const lineY = TOP + plotHeight * ratio;
          return (
            <g key={ratio}>
              <line x1={LEFT} x2={WIDTH - 18} y1={lineY} y2={lineY} stroke="currentColor" className="text-border" />
              <text x={LEFT - 8} y={lineY + 4} textAnchor="end" className="fill-muted-foreground text-[10px]">
                {Math.round(maximum * (1 - ratio))}
              </text>
            </g>
          );
        })}
        <polyline points={created} fill="none" stroke="currentColor" strokeWidth="3" strokeLinejoin="round" className="text-foreground" />
        <polyline points={resolved} fill="none" stroke="#059669" strokeWidth="3" strokeLinejoin="round" />
        {months.map((month, index) => (
          <g key={`${month.year}-${month.month}`}>
            <circle cx={x(index)} cy={y(month.createdTickets)} r="4" fill="currentColor" className="text-foreground" />
            <circle cx={x(index)} cy={y(month.resolvedTickets)} r="4" fill="#059669" />
            {(index % labelStep === 0 || index === months.length - 1) && (
              <text x={x(index)} y={HEIGHT - 12} textAnchor="middle" className="fill-muted-foreground text-[10px]">
                {month.label}
              </text>
            )}
          </g>
        ))}
      </svg>
    </div>
  );
}
