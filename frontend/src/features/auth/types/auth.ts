export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
}

export interface AuthUser {
  email: string;
  roles: string[];
}
