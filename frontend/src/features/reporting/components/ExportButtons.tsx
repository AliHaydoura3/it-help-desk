import { FileDown, FileSpreadsheet, LoaderCircle } from "lucide-react";

import { Button } from "@/components/ui/button";
import type { ReportExportFormat } from "../types/reporting";

interface ExportButtonsProps {
  isPending: boolean;
  pendingFormat: ReportExportFormat | null;
  onExport: (format: ReportExportFormat) => void;
}

export function ExportButtons({ isPending, pendingFormat, onExport }: ExportButtonsProps) {
  return (
    <div className="flex gap-2">
      <Button disabled={isPending} onClick={() => onExport("Pdf")} variant="outline">
        {pendingFormat === "Pdf" ? <LoaderCircle className="animate-spin" /> : <FileDown />}
        PDF
      </Button>
      <Button disabled={isPending} onClick={() => onExport("Excel")} variant="outline">
        {pendingFormat === "Excel" ? <LoaderCircle className="animate-spin" /> : <FileSpreadsheet />}
        Excel
      </Button>
    </div>
  );
}
