import { createContext } from "react";
import type { AuthUser } from "../types/auth";

export interface AuthContextType {
  accessToken: string | null;
  isAuthenticated: boolean;
  user: AuthUser | null;

  login: (token: string) => void;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextType | null>(null);
