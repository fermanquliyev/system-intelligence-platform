FROM node:22-alpine AS build
WORKDIR /app
COPY angular/package.json angular/package-lock.json* ./
COPY angular/.npmrc* ./
RUN npm ci
COPY angular/ .
RUN npm run build:prod

FROM nginx:alpine
COPY --from=build /app/dist/SystemIntelligencePlatform/browser /usr/share/nginx/html
COPY angular/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
