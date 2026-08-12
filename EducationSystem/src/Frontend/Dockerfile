# Multi-stage Dockerfile for React 19 / Vite 8 Frontend SPA
# Stage 1: Build Environment
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files from education-system-ui
COPY education-system-ui/package*.json ./
RUN npm ci

# Copy full application source code
COPY education-system-ui/ ./
RUN npm run build

# Stage 2: Production Nginx Server
FROM nginx:alpine AS final

# Copy compiled static assets to Nginx web root
COPY --from=build /app/dist /usr/share/nginx/html

# Copy Nginx SPA routing fallback configuration
COPY src/Frontend/nginx.conf /etc/nginx/conf.d/default.conf

# Expose HTTP port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
