import { useEffect, useMemo, useState, type PropsWithChildren } from "react";
import { AuthContext } from "./AuthContext";
import {
  getAccessToken,
  removeSessionTokens,
  setSessionTokens,
} from "../utils/tokenStorage";
import { parseAccessToken } from "../utils/parseAccessToken";

export function AuthProvider({ children }: PropsWithChildren) {
  const [accessToken, setToken] = useState<string | null>(getAccessToken());

  function login(accessToken: string, refreshToken: string) {
    setSessionTokens(accessToken, refreshToken);
    setToken(accessToken);
  }

  function logout() {
    removeSessionTokens();
    setToken(null);
  }

  useEffect(() => {
    function sessionUpdated() {
      setToken(getAccessToken());
    }

    window.addEventListener("auth-session-updated", sessionUpdated);
    return () => window.removeEventListener("auth-session-updated", sessionUpdated);
  }, []);

  const value = useMemo(
    () => ({
      accessToken,
      isAuthenticated: accessToken !== null,
      user: parseAccessToken(accessToken),
      login,
      logout,
    }),
    [accessToken],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
