FROM nginx:latest

RUN apt-get update && apt-get install -y curl unzip && \
    curl -LO https://releases.hashicorp.com/consul-template/0.30.0/consul-template_0.30.0_linux_amd64.zip && \
    unzip consul-template_0.30.0_linux_amd64.zip -d /usr/local/bin && \
    rm consul-template_0.30.0_linux_amd64.zip

COPY nginx.conf /etc/nginx/nginx.conf
COPY templates/ /etc/nginx/templates/
COPY consul-template.conf /etc/consul-template/consul-template.conf

CMD ["sh", "-c", "consul-template -config=/etc/consul-template/consul-template.conf & nginx -g 'daemon off;'"]