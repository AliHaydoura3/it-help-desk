import { useState } from "react";
import { CheckCircle2, LockKeyhole, Shield } from "lucide-react";
import { Link, useSearchParams } from "react-router-dom";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { resetPassword } from "../api/passwordRecovery";

export default function ResetPasswordPage() {
  const [params] = useSearchParams();
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [pending, setPending] = useState(false);
  const [complete, setComplete] = useState(false);
  const email = params.get("email") ?? "";
  const token = params.get("token") ?? "";
  const validLink = email.length > 0 && token.length > 0;

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    if (password.length < 8) {
      toast.error("Password must contain at least 8 characters.");
      return;
    }
    if (password !== confirmPassword) {
      toast.error("Passwords do not match.");
      return;
    }

    setPending(true);
    try {
      await resetPassword({ email, token, newPassword: password });
      setComplete(true);
    } catch {
      toast.error("This reset link is invalid or has expired.");
    } finally {
      setPending(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-muted/30 px-4">
      <Card className="w-full max-w-md border-0 shadow-xl">
        <CardHeader className="text-center">
          <div className="mx-auto mb-3 flex size-14 items-center justify-center rounded-full bg-primary text-primary-foreground">
            {complete ? <CheckCircle2 className="size-6" /> : <Shield className="size-6" />}
          </div>
          <CardTitle className="text-2xl">{complete ? "Password updated" : "Set a new password"}</CardTitle>
        </CardHeader>
        <CardContent className="px-6 pb-6">
          {complete ? (
            <div className="text-center">
              <p className="text-sm text-muted-foreground">Your password has been reset. You can now sign in.</p>
              <Button className="mt-6" render={<Link to="/login" />}>Sign in</Button>
            </div>
          ) : !validLink ? (
            <div className="text-center">
              <p className="text-sm text-destructive">This password reset link is incomplete.</p>
              <Button className="mt-6" render={<Link to="/forgot-password" />} variant="outline">Request another link</Button>
            </div>
          ) : (
            <form className="space-y-4" onSubmit={submit}>
              <PasswordInput label="New password" value={password} onChange={setPassword} />
              <PasswordInput label="Confirm password" value={confirmPassword} onChange={setConfirmPassword} />
              <Button className="mt-2 w-full" disabled={pending} size="lg" type="submit">
                {pending ? "Updating..." : "Reset password"}
              </Button>
            </form>
          )}
        </CardContent>
      </Card>
    </main>
  );
}

function PasswordInput({ label, value, onChange }: { label: string; value: string; onChange: (value: string) => void }) {
  return (
    <div className="space-y-2">
      <Label>{label}</Label>
      <div className="relative">
        <LockKeyhole className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input className="pl-10" minLength={8} required type="password" value={value} onChange={(event) => onChange(event.target.value)} />
      </div>
    </div>
  );
}
