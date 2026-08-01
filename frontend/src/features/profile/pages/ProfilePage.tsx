import { useEffect, useState } from "react";
import { ArrowLeft, KeyRound, LoaderCircle, Save, UserRound } from "lucide-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate } from "react-router-dom";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { changePassword, getProfile, updateProfile } from "../api/profile";

export default function ProfilePage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const profileQuery = useQuery({ queryKey: ["profile"], queryFn: getProfile });
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [email, setEmail] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  useEffect(() => {
    if (!profileQuery.data) return;
    setFirstName(profileQuery.data.firstName);
    setLastName(profileQuery.data.lastName);
    setEmail(profileQuery.data.email);
  }, [profileQuery.data]);

  const updateMutation = useMutation({
    mutationFn: updateProfile,
    onSuccess: (profile) => queryClient.setQueryData(["profile"], profile),
  });
  const passwordMutation = useMutation({ mutationFn: changePassword });

  async function saveProfile(event: React.FormEvent) {
    event.preventDefault();
    try {
      await updateMutation.mutateAsync({ firstName, lastName, email });
      toast.success("Profile updated.");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to update your profile."));
    }
  }

  async function savePassword(event: React.FormEvent) {
    event.preventDefault();
    if (newPassword.length < 8) {
      toast.error("The new password must contain at least 8 characters.");
      return;
    }
    if (newPassword !== confirmPassword) {
      toast.error("New passwords do not match.");
      return;
    }
    try {
      await passwordMutation.mutateAsync({ currentPassword, newPassword });
      toast.success("Password changed. Please sign in again.");
      auth.logout();
      navigate("/login", { replace: true });
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to change your password."));
    }
  }

  return (
    <main className="min-h-screen bg-muted/35 px-4 py-8 sm:px-6">
      <div className="mx-auto max-w-4xl">
        <Button render={<Link to="/" />} variant="ghost"><ArrowLeft /> Back to dashboard</Button>
        <div className="mt-5">
          <h1 className="text-3xl font-semibold tracking-tight">Your profile</h1>
          <p className="mt-2 text-sm text-muted-foreground">Manage your personal details and password.</p>
        </div>

        {profileQuery.isLoading ? (
          <div className="mt-8 flex justify-center py-20"><LoaderCircle className="animate-spin text-muted-foreground" /></div>
        ) : profileQuery.isError ? (
          <Card className="mt-8"><CardContent className="py-10 text-center text-sm text-destructive">Unable to load your profile.</CardContent></Card>
        ) : (
          <div className="mt-8 grid gap-6 lg:grid-cols-2">
            <Card className="shadow-sm">
              <CardHeader><CardTitle className="flex items-center gap-2"><UserRound className="size-5" /> Personal information</CardTitle></CardHeader>
              <CardContent>
                <form className="space-y-4" onSubmit={saveProfile}>
                  <div className="grid gap-4 sm:grid-cols-2">
                    <Field label="First name"><Input required maxLength={100} value={firstName} onChange={(event) => setFirstName(event.target.value)} /></Field>
                    <Field label="Last name"><Input required maxLength={100} value={lastName} onChange={(event) => setLastName(event.target.value)} /></Field>
                  </div>
                  <Field label="Email address"><Input required type="email" value={email} onChange={(event) => setEmail(event.target.value)} /></Field>
                  <div className="rounded-lg bg-muted/50 px-3 py-2 text-xs text-muted-foreground">Roles: {profileQuery.data?.roles.join(", ")}</div>
                  <Button disabled={updateMutation.isPending} type="submit">{updateMutation.isPending ? <LoaderCircle className="animate-spin" /> : <Save />} Save profile</Button>
                </form>
              </CardContent>
            </Card>

            <Card className="shadow-sm">
              <CardHeader><CardTitle className="flex items-center gap-2"><KeyRound className="size-5" /> Change password</CardTitle></CardHeader>
              <CardContent>
                <form className="space-y-4" onSubmit={savePassword}>
                  <Field label="Current password"><Input required type="password" value={currentPassword} onChange={(event) => setCurrentPassword(event.target.value)} /></Field>
                  <Field label="New password"><Input required minLength={8} type="password" value={newPassword} onChange={(event) => setNewPassword(event.target.value)} /></Field>
                  <Field label="Confirm new password"><Input required minLength={8} type="password" value={confirmPassword} onChange={(event) => setConfirmPassword(event.target.value)} /></Field>
                  <Button disabled={passwordMutation.isPending} type="submit" variant="outline">{passwordMutation.isPending && <LoaderCircle className="animate-spin" />} Update password</Button>
                </form>
              </CardContent>
            </Card>
          </div>
        )}
      </div>
    </main>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return <div className="space-y-2"><Label>{label}</Label>{children}</div>;
}
