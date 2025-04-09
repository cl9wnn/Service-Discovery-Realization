FROM nginx:1.23-alpine

RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf /etc/nginx/nginx.conf

HEALTHCHECK --interval=5s --timeout=3s \
  CMD curl -f http://localhost/health || exit 1