import { z } from "zod";
import { USER_ROLES } from "@/features/auth/authorization/roles";

const userFields = z.object({
  firstName: z.string().trim().min(1, "First name is required").max(100),
  lastName: z.string().trim().min(1, "Last name is required").max(100),
  email: z.string().trim().email("Enter a valid email address"),
  password: z.string(),
  role: z.enum(USER_ROLES, "Select a role"),
  isActive: z.boolean(),
});

export const createUserSchema = userFields.refine(
  (value) => value.password.length >= 8,
  {
    path: ["password"],
    message: "Password must contain at least 8 characters",
  },
);

export const updateUserSchema = userFields;

export type UserFormData = z.infer<typeof userFields>;
