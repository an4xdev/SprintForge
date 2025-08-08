FROM quay.io/minio/minio:latest

USER root

RUN curl -o /usr/local/bin/mc https://dl.min.io/client/mc/release/linux-amd64/mc && \
    chmod +x /usr/local/bin/mc

COPY minio-init.sh /usr/local/bin/minio-init.sh
RUN chmod +x /usr/local/bin/minio-init.sh

COPY minio-entrypoint.sh /usr/local/bin/minio-entrypoint.sh
RUN chmod +x /usr/local/bin/minio-entrypoint.sh

ENTRYPOINT ["/usr/local/bin/minio-entrypoint.sh"]
