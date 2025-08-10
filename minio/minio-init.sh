#!/bin/bash

set -e

echo "=== MinIO Bucket Initialization Script ==="
echo "Waiting for MinIO to be ready..."

until curl -f http://localhost:9000/minio/health/live > /dev/null 2>&1; do
    echo "MinIO is not ready yet, waiting..."
    sleep 2
done

echo "MinIO is ready, starting initialization..."

mc alias set local http://localhost:9000 ${MINIO_ROOT_USER} ${MINIO_ROOT_PASSWORD}

if mc ls local/uploads > /dev/null 2>&1; then
    echo "Bucket 'uploads' already exists."
else
    echo "Creating 'uploads' bucket..."
    mc mb local/uploads
    echo "Bucket 'uploads' has been created."
fi

echo "Setting public policy for 'uploads' bucket..."
mc anonymous set public local/uploads

if mc ls local/uploads > /dev/null 2>&1; then
    echo "'uploads' bucket is available and configured as public"
    echo "URL: http://localhost:9000/uploads/"
else
    echo "There was a problem configuring the bucket"
    exit 1
fi

mc alias remove local

echo "=== MinIO initialization completed ==="
