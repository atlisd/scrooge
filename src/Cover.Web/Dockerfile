FROM node:22-alpine AS build
WORKDIR /app
COPY src/Cover.Web/package.json src/Cover.Web/package-lock.json* ./
RUN npm ci || npm install
COPY src/Cover.Web/ .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY src/Cover.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
