# syntax=docker/dockerfile:1
FROM node:22-alpine AS build
WORKDIR /web

COPY src/Frontend/archicore-web/package.json ./
RUN npm install

COPY src/Frontend/archicore-web/ ./
RUN npm run build

FROM nginx:1.29-alpine AS runtime
COPY --from=build /web/dist/ /usr/share/nginx/html/
COPY src/Frontend/archicore-web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
