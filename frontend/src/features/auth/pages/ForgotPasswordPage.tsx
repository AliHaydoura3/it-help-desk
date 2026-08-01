import { useState } from "react";
import { ArrowLeft, Mail, Send, Shield } from "lucide-react";
import { Link } from "react-router-dom";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { forgotPassword } from "../api/passwordRecovery";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [pending, setPending] = useState(false);
  const [sent, setSent] = useState(false);

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    setPending(true);
    try {
      await forgotPassword(email);
      setSent(true);
    } catch {
      toast.error("Unable to submit the request. Please try again.");
    } finally {
      setPending(false);
    }
  }

  return (
    <main className="flex min-h-screen items-center justify-center bg-muted/30 px-4">
      <Card className="w-full max-w-md border-0 shadow-xl">
        <CardHeader className="text-center">
          <div className="mx-auto mb-3 flex size-14 items-center justify-center rounded-full bg-primary text-primary-foreground">
            {sent ? <Send className="size-6" /> : <Shield className="size-6" />}
          </div>
          <CardTitle className="text-2xl">{sent ? "Check your email" : "Forgot password?"}</CardTitle>
        </CardHeader>
        <CardContent className="px-6 pb-6">
          {sent ? (
            <div className="text-center">
              <p className="text-sm leading-6 text-muted-foreground">
                If an active account exists for <span className="font-medium text-foreground">{email}</span>, a reset link has been sent.
              </p>
              <Button className="mt-6" render={<Link to="/login" />} variant="outline">
                <ArrowLeft /> Back to sign in
              </Button>
            </div>
          ) : (
            <form className="space-y-5" onSubmit={submit}>
              <p className="text-center text-sm leading-6 text-muted-foreground">
                Enter your account email and we’ll send you a secure reset link.
              </p>
              <div className="space-y-2">
                <Label htmlFor="recovery-email">Email address</Label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                  <Input id="recovery-email" className="pl-10" required type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
                </div>
              </div>
              <Button className="w-full" disabled={pending} size="lg" type="submit">
                {pending ? "Sending..." : "Send reset link"}
              </Button>
              <Link className="flex items-center justify-center gap-2 text-sm text-muted-foreground hover:text-foreground" to="/login">
                <ArrowLeft className="size-4" /> Back to sign in
              </Link>
            </form>
          )}
        </CardContent>
      </Card>
    </main>
  );
}
