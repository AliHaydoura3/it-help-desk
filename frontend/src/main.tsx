import React from "react";
import ReactDOM from "react-dom/client";
import App from "./App";
import "./index.css";
import { QueryProvider } from "@/app/providers/QueryProvider";
import { AuthProvider } from "./features/auth/context/AuthProvider";
import { Toaster } from "@/components/ui/sonner";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <QueryProvider>
      <AuthProvider>
        <App />
        <Toaster />
      </AuthProvider>
    </QueryProvider>
  </React.StrictMode>,
);
