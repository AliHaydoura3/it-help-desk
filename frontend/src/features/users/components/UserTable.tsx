import {
  MoreHorizontal,
  Pencil,
  RotateCcw,
  SearchX,
  UserRoundX,
} from "lucide-react";
import { useState } from "react";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { User } from "../types/user";

interface UserTableProps {
  users: User[];
  reactivatingId: string | null;
  onEdit: (user: User) => void;
  onDeactivate: (user: User) => void;
  onReactivate: (user: User) => void;
}

const roleLabels: Record<string, string> = {
  Admin: "Admin",
  ITSupportSpecialist: "IT Support",
  Manager: "Manager",
  Employee: "Employee",
};

export function UserTable({
  users,
  reactivatingId,
  onEdit,
  onDeactivate,
  onReactivate,
}: UserTableProps) {
  if (users.length === 0) {
    return (
      <div className="flex min-h-72 flex-col items-center justify-center px-6 text-center">
        <div className="flex size-12 items-center justify-center rounded-full bg-muted text-muted-foreground">
          <SearchX className="size-5" />
        </div>
        <h3 className="mt-4 font-medium">No users found</h3>
        <p className="mt-1 text-sm text-muted-foreground">
          Try changing your search or status filter.
        </p>
      </div>
    );
  }

  return (
    <>
      <div className="hidden overflow-x-auto md:block">
        <table className="w-full text-left text-sm">
          <thead className="border-b bg-muted/35 text-xs font-medium uppercase tracking-wide text-muted-foreground">
            <tr>
              <th className="px-5 py-3.5">User</th>
              <th className="px-5 py-3.5">Role</th>
              <th className="px-5 py-3.5">Status</th>
              <th className="w-16 px-5 py-3.5 text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {users.map((user) => (
              <tr className="transition-colors hover:bg-muted/25" key={user.id}>
                <td className="px-5 py-4">
                  <UserIdentity user={user} />
                </td>
                <td className="px-5 py-4">
                  <RoleList roles={user.roles} />
                </td>
                <td className="px-5 py-4">
                  <StatusBadge active={user.isActive} />
                </td>
                <td className="px-5 py-4 text-right">
                  <UserActions
                    onDeactivate={onDeactivate}
                    onEdit={onEdit}
                    onReactivate={onReactivate}
                    reactivating={reactivatingId === user.id}
                    user={user}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="divide-y md:hidden">
        {users.map((user) => (
          <div className="space-y-4 p-4" key={user.id}>
            <div className="flex items-start justify-between gap-3">
              <UserIdentity user={user} />
              <UserActions
                onDeactivate={onDeactivate}
                onEdit={onEdit}
                onReactivate={onReactivate}
                reactivating={reactivatingId === user.id}
                user={user}
              />
            </div>
            <div className="flex flex-wrap items-center justify-between gap-2">
              <RoleList roles={user.roles} />
              <StatusBadge active={user.isActive} />
            </div>
          </div>
        ))}
      </div>
    </>
  );
}

function UserIdentity({ user }: { user: User }) {
  const initials = `${user.firstName[0] ?? ""}${user.lastName[0] ?? ""}`;

  return (
    <div className="flex min-w-0 items-center gap-3">
      <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-primary/8 text-xs font-semibold text-primary ring-1 ring-primary/10">
        {initials.toUpperCase()}
      </div>
      <div className="min-w-0">
        <p className="truncate font-medium">
          {user.firstName} {user.lastName}
        </p>
        <p className="truncate text-xs text-muted-foreground">{user.email}</p>
      </div>
    </div>
  );
}

function RoleList({ roles }: { roles: string[] }) {
  return (
    <div className="flex flex-wrap gap-1.5">
      {roles.map((role) => (
        <span className="rounded-md bg-secondary px-2 py-1 text-xs font-medium text-secondary-foreground" key={role}>
          {roleLabels[role] ?? role}
        </span>
      ))}
    </div>
  );
}

function StatusBadge({ active }: { active: boolean }) {
  return (
    <span
      className={cn(
        "inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium",
        active
          ? "bg-emerald-500/10 text-emerald-700 dark:text-emerald-400"
          : "bg-muted text-muted-foreground",
      )}
    >
      <span className={cn("size-1.5 rounded-full", active ? "bg-emerald-500" : "bg-muted-foreground/60")} />
      {active ? "Active" : "Inactive"}
    </span>
  );
}

function UserActions({
  user,
  reactivating,
  onEdit,
  onDeactivate,
  onReactivate,
}: {
  user: User;
  reactivating: boolean;
  onEdit: (user: User) => void;
  onDeactivate: (user: User) => void;
  onReactivate: (user: User) => void;
}) {
  const [open, setOpen] = useState(false);

  return (
    <div className="relative inline-block">
      <Button aria-label={`Actions for ${user.firstName}`} onClick={() => setOpen((value) => !value)} size="icon" variant="ghost">
        <MoreHorizontal />
      </Button>
      {open && (
        <div className="absolute right-0 top-9 z-20 w-40 rounded-xl border bg-popover p-1.5 text-left shadow-lg">
          <button
            className="flex w-full items-center gap-2 rounded-lg px-2.5 py-2 text-sm hover:bg-muted"
            onClick={() => {
              setOpen(false);
              onEdit(user);
            }}
            type="button"
          >
            <Pencil className="size-4" /> Edit user
          </button>
          {user.isActive ? (
            <button
              className="flex w-full items-center gap-2 rounded-lg px-2.5 py-2 text-sm text-destructive hover:bg-destructive/10"
              onClick={() => {
                setOpen(false);
                onDeactivate(user);
              }}
              type="button"
            >
              <UserRoundX className="size-4" /> Deactivate
            </button>
          ) : (
            <button
              className="flex w-full items-center gap-2 rounded-lg px-2.5 py-2 text-sm hover:bg-muted disabled:opacity-50"
              disabled={reactivating}
              onClick={() => {
                setOpen(false);
                onReactivate(user);
              }}
              type="button"
            >
              <RotateCcw className={cn("size-4", reactivating && "animate-spin")} /> Reactivate
            </button>
          )}
        </div>
      )}
    </div>
  );
}
