FROM node:22-alpine AS build
WORKDIR /app
COPY src/Scrooge.Web/package.json src/Scrooge.Web/package-lock.json* ./
RUN npm ci || npm install
COPY src/Scrooge.Web/ .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY src/Scrooge.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
