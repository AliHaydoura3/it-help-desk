import axios from "axios";
import type { InternalAxiosRequestConfig } from "axios";
import type { LoginResponse } from "@/features/auth/types/auth";
import {
    getAccessToken,
    getRefreshToken,
    removeSessionTokens,
    setSessionTokens,
} from "@/features/auth/utils/tokenStorage";

export const apiClient = axios.create({
    baseURL: import.meta.env.VITE_API_URL,
    headers: {
        "Content-Type": "application/json",
    },
});

apiClient.interceptors.request.use((config) => {
    const token = getAccessToken();

    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }

    return config;
});

interface RetryableRequest extends InternalAxiosRequestConfig {
    _retry?: boolean;
}

let refreshRequest: Promise<LoginResponse> | null = null;

apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const request = error.config as RetryableRequest | undefined;
        const refreshToken = getRefreshToken();

        if (error.response?.status !== 401 || !request || request._retry || !refreshToken) {
            return Promise.reject(error);
        }

        request._retry = true;

        try {
            refreshRequest ??= axios
                .post<LoginResponse>(
                    `${apiClient.defaults.baseURL}/auth/refresh`,
                    { refreshToken },
                    { headers: { "Content-Type": "application/json" } },
                )
                .then((response) => response.data)
                .finally(() => {
                    refreshRequest = null;
                });

            const session = await refreshRequest;
            setSessionTokens(session.accessToken, session.refreshToken);
            window.dispatchEvent(new Event("auth-session-updated"));
            request.headers.Authorization = `Bearer ${session.accessToken}`;

            return apiClient(request);
        } catch (refreshError) {
            removeSessionTokens();
            window.dispatchEvent(new Event("auth-session-updated"));
            return Promise.reject(refreshError);
        }
    },
);
