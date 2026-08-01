const TOKEN_KEY = "access_token";
const REFRESH_TOKEN_KEY = "refresh_token";

export function getAccessToken(): string | null {
  return sessionStorage.getItem(TOKEN_KEY);
}

export function setAccessToken(token: string): void {
  sessionStorage.setItem(TOKEN_KEY, token);
}

export function removeAccessToken(): void {
  sessionStorage.removeItem(TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  return sessionStorage.getItem(REFRESH_TOKEN_KEY);
}

export function setRefreshToken(token: string): void {
  sessionStorage.setItem(REFRESH_TOKEN_KEY, token);
}

export function setSessionTokens(
  accessToken: string,
  refreshToken: string,
): void {
  setAccessToken(accessToken);
  setRefreshToken(refreshToken);
}

export function removeSessionTokens(): void {
  removeAccessToken();
  sessionStorage.removeItem(REFRESH_TOKEN_KEY);
}
