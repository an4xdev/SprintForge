#!/bin/bash

echo "Starting MinIO with automatic bucket initialization..."

minio server /data --console-address "${MINIO_CONSOLE_ADDRESS}" &
MINIO_PID=$!

(
    sleep 5
    /usr/local/bin/minio-init.sh
) &

wait $MINIO_PID
