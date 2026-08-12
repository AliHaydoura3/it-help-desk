import { useEffect, useState } from "react";
import {
  Activity,
  ChevronLeft,
  ChevronRight,
  Search,
  ShieldCheck,
  UserRoundCheck,
  UserRoundPlus,
  Users,
  UserX,
} from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { DeactivateUserDialog } from "@/features/users/components/DeactivateUserDialog";
import { UserFormDialog } from "@/features/users/components/UserFormDialog";
import { UserTable } from "@/features/users/components/UserTable";
import {
  useCreateUser,
  useDeactivateUser,
  useUpdateUser,
  useUsers,
} from "@/features/users/hooks/useUsers";
import type { User } from "@/features/users/types/user";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import type { UserFormData } from "@/features/users/validation/userSchema";

type StatusFilter = "all" | "active" | "inactive";
const EMPTY_USERS: User[] = [];
const PAGE_SIZE = 10;

export default function DashboardPage() {
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState<StatusFilter>("all");
  const [pageNumber, setPageNumber] = useState(1);
  const [formOpen, setFormOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [deactivatingUser, setDeactivatingUser] = useState<User | null>(null);
  const [reactivatingId, setReactivatingId] = useState<string | null>(null);
  const debouncedSearch = useDebouncedValue(search, 300);
  const usersQuery = useUsers({
    pageNumber,
    pageSize: PAGE_SIZE,
    search: debouncedSearch.trim() || undefined,
    isActive:
      status === "all" ? undefined : status === "active",
  });
  const createMutation = useCreateUser();
  const updateMutation = useUpdateUser();
  const deactivateMutation = useDeactivateUser();

  const users = usersQuery.data?.items ?? EMPTY_USERS;

  useEffect(() => {
    const totalPages = usersQuery.data?.totalPages ?? 0;

    if (totalPages > 0 && pageNumber > totalPages) {
      setPageNumber(totalPages);
    }
  }, [pageNumber, usersQuery.data?.totalPages]);

  function openCreateForm() {
    setEditingUser(null);
    setFormOpen(true);
  }

  function openEditForm(user: User) {
    setEditingUser(user);
    setFormOpen(true);
  }

  async function submitUser(values: UserFormData) {
    try {
      if (editingUser) {
        await updateMutation.mutateAsync({
          id: editingUser.id,
          request: {
            firstName: values.firstName,
            lastName: values.lastName,
            email: values.email,
            isActive: values.isActive,
            role: values.role,
          },
        });
        toast.success("User updated successfully.");
      } else {
        await createMutation.mutateAsync({
          firstName: values.firstName,
          lastName: values.lastName,
          email: values.email,
          password: values.password,
          role: values.role,
        });
        toast.success("User created successfully.");
      }

      setFormOpen(false);
      setEditingUser(null);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to save this user."));
    }
  }

  async function confirmDeactivation() {
    if (!deactivatingUser) return;

    try {
      await deactivateMutation.mutateAsync(deactivatingUser.id);
      toast.success("User deactivated.");
      setDeactivatingUser(null);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to deactivate this user."));
    }
  }

  async function reactivateUser(user: User) {
    setReactivatingId(user.id);
    try {
      await updateMutation.mutateAsync({
        id: user.id,
        request: {
          firstName: user.firstName,
          lastName: user.lastName,
          email: user.email,
          isActive: true,
          role: user.role,
        },
      });
      toast.success("User reactivated.");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to reactivate this user."));
    } finally {
      setReactivatingId(null);
    }
  }

  return (
    <>
      <main className="mx-auto max-w-7xl px-4 py-7 sm:px-6 lg:px-8 lg:py-9">
          <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
            <div>
              <p className="text-sm font-medium text-muted-foreground">Access control</p>
              <h1 className="mt-1 text-2xl font-semibold tracking-tight sm:text-3xl">User management</h1>
              <p className="mt-2 max-w-2xl text-sm text-muted-foreground">
                Create accounts, assign one role, and control access to the help desk.
              </p>
            </div>
            <Button className="h-10 px-4" onClick={openCreateForm}>
              <UserRoundPlus /> Add user
            </Button>
          </div>

          <div className="mt-7 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <StatCard icon={Users} label="Total users" value={(usersQuery.data?.activeCount ?? 0) + (usersQuery.data?.inactiveCount ?? 0)} />
            <StatCard icon={UserRoundCheck} label="Active" tone="success" value={usersQuery.data?.activeCount ?? 0} />
            <StatCard icon={UserX} label="Inactive" tone="muted" value={usersQuery.data?.inactiveCount ?? 0} />
            <StatCard icon={ShieldCheck} label="Administrators" value={usersQuery.data?.administratorCount ?? 0} />
          </div>

          <Card className="mt-6 gap-0 py-0 shadow-sm">
            <div className="flex flex-col gap-3 border-b p-4 sm:flex-row sm:items-center sm:justify-between">
              <div className="relative w-full sm:max-w-sm">
                <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  className="h-9 pl-9"
                  onChange={(event) => {
                    setSearch(event.target.value);
                    setPageNumber(1);
                  }}
                  placeholder="Search by name or email..."
                  value={search}
                />
              </div>
              <select
                aria-label="Filter users by status"
                className="h-9 rounded-lg border border-input bg-background px-3 text-sm outline-none focus:border-ring focus:ring-3 focus:ring-ring/30"
                onChange={(event) => {
                  setStatus(event.target.value as StatusFilter);
                  setPageNumber(1);
                }}
                value={status}
              >
                <option value="all">All statuses</option>
                <option value="active">Active</option>
                <option value="inactive">Inactive</option>
              </select>
            </div>

            {usersQuery.isLoading ? (
              <LoadingTable />
            ) : usersQuery.isError ? (
              <div className="flex min-h-72 flex-col items-center justify-center px-6 text-center">
                <div className="flex size-12 items-center justify-center rounded-full bg-destructive/10 text-destructive"><Activity className="size-5" /></div>
                <h3 className="mt-4 font-medium">Could not load users</h3>
                <p className="mt-1 text-sm text-muted-foreground">Check your connection and try again.</p>
                <Button className="mt-4" onClick={() => usersQuery.refetch()} variant="outline">Try again</Button>
              </div>
            ) : (
              <UserTable
                onDeactivate={setDeactivatingUser}
                onEdit={openEditForm}
                onReactivate={reactivateUser}
                reactivatingId={reactivatingId}
                users={users}
              />
            )}

            {!usersQuery.isLoading && !usersQuery.isError && (usersQuery.data?.totalCount ?? 0) > 0 && (
              <Pagination
                onPageChange={setPageNumber}
                pageNumber={usersQuery.data?.pageNumber ?? pageNumber}
                pageSize={usersQuery.data?.pageSize ?? PAGE_SIZE}
                totalCount={usersQuery.data?.totalCount ?? 0}
                totalPages={usersQuery.data?.totalPages ?? 0}
              />
            )}
          </Card>
      </main>

      <UserFormDialog
        isPending={createMutation.isPending || updateMutation.isPending}
        onClose={() => {
          setFormOpen(false);
          setEditingUser(null);
        }}
        onSubmit={submitUser}
        open={formOpen}
        user={editingUser}
      />
      <DeactivateUserDialog
        isPending={deactivateMutation.isPending}
        onClose={() => setDeactivatingUser(null)}
        onConfirm={confirmDeactivation}
        user={deactivatingUser}
      />
    </>
  );
}

function Pagination({
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  onPageChange,
}: {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  const firstItem = (pageNumber - 1) * pageSize + 1;
  const lastItem = Math.min(pageNumber * pageSize, totalCount);
  const visiblePages = Array.from(
    new Set([1, pageNumber - 1, pageNumber, pageNumber + 1, totalPages]),
  ).filter((page) => page >= 1 && page <= totalPages);

  return (
    <div className="flex flex-col gap-3 border-t px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
      <p className="text-xs text-muted-foreground">
        Showing {firstItem}–{lastItem} of {totalCount} users
      </p>
      <div className="flex items-center gap-1">
        <Button
          aria-label="Previous page"
          disabled={pageNumber === 1}
          onClick={() => onPageChange(pageNumber - 1)}
          size="icon-sm"
          variant="outline"
        >
          <ChevronLeft />
        </Button>
        {visiblePages.map((page, index) => {
          const previousPage = visiblePages[index - 1];
          const showGap = previousPage !== undefined && page - previousPage > 1;

          return (
            <span className="contents" key={page}>
              {showGap && (
                <span className="flex size-7 items-center justify-center text-xs text-muted-foreground">
                  …
                </span>
              )}
              <Button
                aria-current={page === pageNumber ? "page" : undefined}
                aria-label={`Page ${page}`}
                onClick={() => onPageChange(page)}
                size="icon-sm"
                variant={page === pageNumber ? "default" : "ghost"}
              >
                {page}
              </Button>
            </span>
          );
        })}
        <Button
          aria-label="Next page"
          disabled={pageNumber === totalPages}
          onClick={() => onPageChange(pageNumber + 1)}
          size="icon-sm"
          variant="outline"
        >
          <ChevronRight />
        </Button>
      </div>
    </div>
  );
}

function useDebouncedValue<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const timeout = window.setTimeout(() => setDebouncedValue(value), delay);

    return () => window.clearTimeout(timeout);
  }, [delay, value]);

  return debouncedValue;
}

function StatCard({
  icon: Icon,
  label,
  value,
  tone = "default",
}: {
  icon: typeof Users;
  label: string;
  value: number;
  tone?: "default" | "success" | "muted";
}) {
  return (
    <Card className="gap-0 py-0 shadow-sm">
      <CardContent className="flex items-center gap-4 p-4">
        <div className={`flex size-10 items-center justify-center rounded-xl ${tone === "success" ? "bg-emerald-500/10 text-emerald-700 dark:text-emerald-400" : tone === "muted" ? "bg-muted text-muted-foreground" : "bg-primary/8 text-primary"}`}>
          <Icon className="size-5" />
        </div>
        <div>
          <p className="text-2xl font-semibold leading-none">{value}</p>
          <p className="mt-1.5 text-xs text-muted-foreground">{label}</p>
        </div>
      </CardContent>
    </Card>
  );
}

function LoadingTable() {
  return (
    <div className="divide-y">
      {Array.from({ length: 5 }).map((_, index) => (
        <div className="flex items-center gap-4 px-5 py-4" key={index}>
          <div className="size-10 animate-pulse rounded-full bg-muted" />
          <div className="flex-1 space-y-2">
            <div className="h-3 w-36 animate-pulse rounded bg-muted" />
            <div className="h-2.5 w-52 max-w-full animate-pulse rounded bg-muted" />
          </div>
          <div className="hidden h-6 w-20 animate-pulse rounded bg-muted sm:block" />
        </div>
      ))}
    </div>
  );
}
