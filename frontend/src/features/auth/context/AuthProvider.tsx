import { useMemo, useState, type PropsWithChildren } from "react";
import { AuthContext } from "./AuthContext";
import {
  getAccessToken,
  removeAccessToken,
  setAccessToken,
} from "../utils/tokenStorage";

export function AuthProvider({ children }: PropsWithChildren) {
  const [accessToken, setToken] = useState<string | null>(getAccessToken());

  function login(token: string) {
    setAccessToken(token);
    setToken(token);
  }

  function logout() {
    removeAccessToken();
    setToken(null);
  }

  const value = useMemo(
    () => ({
      accessToken,
      isAuthenticated: accessToken !== null,
      login,
      logout,
    }),
    [accessToken],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
