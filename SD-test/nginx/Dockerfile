# Используем официальный образ Nginx
FROM nginx:1.23-alpine

# Удаляем дефолтную конфигурацию
RUN rm /etc/nginx/conf.d/default.conf

# Копируем нашу кастомную конфигурацию
COPY nginx.conf /etc/nginx/nginx.conf

# Добавляем healthcheck для Docker
HEALTHCHECK --interval=5s --timeout=3s \
  CMD curl -f http://localhost/health || exit 1