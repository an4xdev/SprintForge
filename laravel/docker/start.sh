#!/bin/sh

echo "Waiting for database..."
until nc -z $DB_HOST $DB_PORT; do
    echo "Database not ready, waiting..."
    sleep 2
done

echo "Database is ready"
sleep 1

echo "Optimizing Laravel..."
php artisan config:cache
php artisan route:cache
php artisan view:cache

echo "Starting PHP-FPM..."
exec php-fpm -F
