export interface Profile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  roles: string[];
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  email: string;
}
