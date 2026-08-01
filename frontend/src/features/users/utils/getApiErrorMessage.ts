import axios from "axios";

interface ApiProblem {
  title?: string;
  errors?: Record<string, string[]>;
}

export function getApiErrorMessage(
  error: unknown,
  fallback = "Something went wrong. Please try again.",
): string {
  if (!axios.isAxiosError<ApiProblem>(error)) {
    return fallback;
  }

  const problem = error.response?.data;
  const validationMessage = problem?.errors
    ? Object.values(problem.errors).flat()[0]
    : undefined;

  return validationMessage ?? problem?.title ?? fallback;
}
