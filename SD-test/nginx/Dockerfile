FROM nginx:latest

RUN apt-get update && apt-get install -y unzip

COPY consul-template_0.40.0_linux_amd64.zip /tmp/consul-template.zip

RUN unzip /tmp/consul-template.zip -d /usr/local/bin && \
    rm /tmp/consul-template.zip

COPY nginx.conf /etc/nginx/nginx.conf
COPY templates/ /etc/nginx/templates/
COPY consul-template.conf /etc/consul-template/consul-template.conf

CMD ["sh", "-c", "consul-template -config=/etc/consul-template/consul-template.conf & nginx -g 'daemon off;'"]