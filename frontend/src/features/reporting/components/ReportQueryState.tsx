import { BarChart3, LoaderCircle } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";

export function ReportLoading() {
  return (
    <div className="flex min-h-80 items-center justify-center">
      <div className="text-center text-sm text-muted-foreground">
        <LoaderCircle className="mx-auto mb-3 size-6 animate-spin" />
        Loading analytics…
      </div>
    </div>
  );
}

export function ReportError({ onRetry }: { onRetry: () => void }) {
  return (
    <Card className="shadow-sm">
      <CardContent className="flex min-h-72 flex-col items-center justify-center text-center">
        <div className="flex size-12 items-center justify-center rounded-full bg-destructive/10 text-destructive">
          <BarChart3 className="size-5" />
        </div>
        <h2 className="mt-4 font-medium">Unable to load this report</h2>
        <p className="mt-1 text-sm text-muted-foreground">Check your connection and try again.</p>
        <Button className="mt-4" onClick={onRetry} variant="outline">Try again</Button>
      </CardContent>
    </Card>
  );
}
