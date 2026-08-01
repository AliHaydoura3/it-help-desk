import { useEffect, useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { Eye, EyeOff, LoaderCircle, X } from "lucide-react";
import { useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";
import { USER_ROLES, type User } from "../types/user";
import {
  createUserSchema,
  updateUserSchema,
  type UserFormData,
} from "../validation/userSchema";

interface UserFormDialogProps {
  user: User | null;
  open: boolean;
  isPending: boolean;
  onClose: () => void;
  onSubmit: (values: UserFormData) => Promise<void>;
}

const roleLabels: Record<string, string> = {
  Admin: "Administrator",
  ITSupportSpecialist: "IT Support Specialist",
  Manager: "Manager",
  Employee: "Employee",
};

export function UserFormDialog({
  user,
  open,
  isPending,
  onClose,
  onSubmit,
}: UserFormDialogProps) {
  const [showPassword, setShowPassword] = useState(false);
  const isEditing = user !== null;
  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<UserFormData>({
    resolver: zodResolver(isEditing ? updateUserSchema : createUserSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      roles: ["Employee"],
      isActive: true,
    },
  });

  const selectedRoles = watch("roles");

  useEffect(() => {
    if (!open) return;

    reset(
      user
        ? {
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
            password: "",
            roles: user.roles,
            isActive: user.isActive,
          }
        : {
            firstName: "",
            lastName: "",
            email: "",
            password: "",
            roles: ["Employee"],
            isActive: true,
          },
    );
    setShowPassword(false);
  }, [open, reset, user]);

  useEffect(() => {
    if (!open) return;

    function closeOnEscape(event: KeyboardEvent) {
      if (event.key === "Escape" && !isPending) onClose();
    }

    window.addEventListener("keydown", closeOnEscape);
    return () => window.removeEventListener("keydown", closeOnEscape);
  }, [isPending, onClose, open]);

  if (!open) return null;

  function toggleRole(role: string) {
    const nextRoles = selectedRoles.includes(role)
      ? selectedRoles.filter((selectedRole) => selectedRole !== role)
      : [...selectedRoles, role];

    setValue("roles", nextRoles, { shouldDirty: true, shouldValidate: true });
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/35 p-4 backdrop-blur-[2px]"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && !isPending) onClose();
      }}
    >
      <section
        aria-labelledby="user-dialog-title"
        aria-modal="true"
        role="dialog"
        className="max-h-[calc(100vh-2rem)] w-full max-w-xl overflow-y-auto rounded-2xl bg-card shadow-2xl ring-1 ring-foreground/10"
      >
        <div className="flex items-start justify-between border-b px-6 py-5">
          <div>
            <h2 id="user-dialog-title" className="text-xl font-semibold">
              {isEditing ? "Edit user" : "Add a new user"}
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {isEditing
                ? "Update account details, access, and status."
                : "Create an account and assign its permissions."}
            </p>
          </div>
          <Button
            aria-label="Close dialog"
            disabled={isPending}
            onClick={onClose}
            size="icon"
            type="button"
            variant="ghost"
          >
            <X />
          </Button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="space-y-5 px-6 py-5">
            <div className="grid gap-4 sm:grid-cols-2">
              <FormField label="First name" error={errors.firstName?.message}>
                <Input
                  autoFocus
                  placeholder="Jane"
                  {...register("firstName")}
                />
              </FormField>
              <FormField label="Last name" error={errors.lastName?.message}>
                <Input placeholder="Cooper" {...register("lastName")} />
              </FormField>
            </div>

            <FormField label="Email address" error={errors.email?.message}>
              <Input
                autoComplete="email"
                placeholder="jane@company.com"
                type="email"
                {...register("email")}
              />
            </FormField>

            {!isEditing && (
              <FormField label="Temporary password" error={errors.password?.message}>
                <div className="relative">
                  <Input
                    autoComplete="new-password"
                    className="pr-10"
                    placeholder="At least 8 characters"
                    type={showPassword ? "text" : "password"}
                    {...register("password")}
                  />
                  <button
                    aria-label={showPassword ? "Hide password" : "Show password"}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                    onClick={() => setShowPassword((value) => !value)}
                    type="button"
                  >
                    {showPassword ? <EyeOff className="size-4" /> : <Eye className="size-4" />}
                  </button>
                </div>
              </FormField>
            )}

            <fieldset>
              <legend className="text-sm font-medium">Roles</legend>
              <p className="mt-1 text-xs text-muted-foreground">
                Select one or more roles for this account.
              </p>
              <div className="mt-3 grid gap-2 sm:grid-cols-2">
                {USER_ROLES.map((role) => {
                  const checked = selectedRoles.includes(role);
                  return (
                    <label
                      className={cn(
                        "flex cursor-pointer items-center gap-3 rounded-xl border px-3 py-3 text-sm transition-colors",
                        checked
                          ? "border-primary/30 bg-primary/[0.04]"
                          : "hover:bg-muted/60",
                      )}
                      key={role}
                    >
                      <input
                        checked={checked}
                        className="size-4 accent-primary"
                        onChange={() => toggleRole(role)}
                        type="checkbox"
                      />
                      {roleLabels[role]}
                    </label>
                  );
                })}
              </div>
              {errors.roles && (
                <p className="mt-2 text-sm text-destructive">
                  {errors.roles.message}
                </p>
              )}
            </fieldset>

            {isEditing && (
              <label className="flex items-center justify-between rounded-xl border bg-muted/30 px-4 py-3">
                <span>
                  <span className="block text-sm font-medium">Active account</span>
                  <span className="block text-xs text-muted-foreground">
                    Inactive users cannot sign in.
                  </span>
                </span>
                <input className="size-4 accent-primary" type="checkbox" {...register("isActive")} />
              </label>
            )}
          </div>

          <div className="flex justify-end gap-2 border-t bg-muted/30 px-6 py-4">
            <Button disabled={isPending} onClick={onClose} type="button" variant="outline">
              Cancel
            </Button>
            <Button disabled={isPending} type="submit">
              {isPending && <LoaderCircle className="animate-spin" />}
              {isEditing ? "Save changes" : "Create user"}
            </Button>
          </div>
        </form>
      </section>
    </div>
  );
}

function FormField({
  label,
  error,
  children,
}: {
  label: string;
  error?: string;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-2">
      <Label>{label}</Label>
      {children}
      {error && <p className="text-sm text-destructive">{error}</p>}
    </div>
  );
}
