import { ArrowLeft, ShieldX } from "lucide-react";
import { useNavigate } from "react-router-dom";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAuth } from "../hooks/useAuth";

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
            User management is restricted to administrators. Sign in with an
            administrator account to continue.
          </p>
          <Button className="mt-6" onClick={signOut} variant="outline">
            <ArrowLeft /> Sign out
          </Button>
        </CardContent>
      </Card>
    </main>
  );
}
