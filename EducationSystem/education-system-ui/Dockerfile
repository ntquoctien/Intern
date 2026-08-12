# Multi-stage Dockerfile for React 19 / Vite 8 Frontend SPA
# Stage 1: Build Environment
FROM node:20-alpine AS build

WORKDIR /app

# Install package dependencies using clean install
COPY package*.json ./
RUN npm ci

# Copy source code and build production web bundle
COPY . .
RUN npm run build

# Stage 2: Production Nginx Server
FROM nginx:alpine AS final

# Copy compiled static assets to Nginx web root
COPY --from=build /app/dist /usr/share/nginx/html

# Copy Nginx SPA routing fallback configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose HTTP port
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
