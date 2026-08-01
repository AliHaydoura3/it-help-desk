import { ArrowLeft, House, ShieldX } from "lucide-react";
import { useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "../hooks/useAuth";
import { getDefaultRoute } from "../utils/getDefaultRoute";

export default function AccessDeniedPage() {
  const auth = useAuth();
  const navigate = useNavigate();

  function signOut() {
    auth.logout();
    navigate("/login", { replace: true });
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-muted/35 p-4">
      <Card className="w-full max-w-md shadow-xl">
        <CardContent className="px-8 py-10 text-center">
          <div className="mx-auto flex size-14 items-center justify-center rounded-full bg-destructive/10 text-destructive">
            <ShieldX className="size-7" />
          </div>
          <h1 className="mt-5 text-2xl font-semibold">Access denied</h1>
          <p className="mt-2 text-sm leading-6 text-muted-foreground">
            Your account does not have permission to open this page. You can
            return to your available workspace or sign out.
          </p>
          <div className="mt-6 flex flex-col justify-center gap-2 sm:flex-row">
            <Button onClick={() => navigate(getDefaultRoute(auth.user), { replace: true })}>
              <House /> Return to workspace
            </Button>
            <Button onClick={signOut} variant="outline">
              <ArrowLeft /> Sign out
            </Button>
          </div>
        </CardContent>
      </Card>
    </main>
  );
}
