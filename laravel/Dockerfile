FROM composer:2.7 AS build

WORKDIR /app

COPY composer.json composer.lock ./
RUN composer install --no-dev --no-scripts --prefer-dist --no-interaction --optimize-autoloader --ignore-platform-req=ext-sockets --ignore-platform-req=php

COPY . .

FROM php:8.2-fpm-alpine AS final

WORKDIR /var/www

RUN apk add --no-cache \
    libpng \
    libpq \
    libzip \
    postgresql-dev \
    netcat-openbsd \
    && apk add --no-cache --virtual .build-deps \
    libpng-dev \
    libjpeg-turbo-dev \
    libzip-dev \
    linux-headers \
    && docker-php-ext-configure zip \
    && docker-php-ext-install \
    pdo \
    pdo_pgsql \
    pgsql \
    zip \
    gd \
    sockets \
    && apk del .build-deps

RUN echo "memory_limit=256M" > /usr/local/etc/php/conf.d/memory-limit.ini \
    && echo "upload_max_filesize=64M" >> /usr/local/etc/php/conf.d/uploads.ini \
    && echo "post_max_size=64M" >> /usr/local/etc/php/conf.d/uploads.ini

COPY --from=build /app /var/www

COPY docker/www.conf /usr/local/etc/php-fpm.d/www.conf
COPY docker/start.sh /usr/local/bin/start.sh

RUN chown -R www-data:www-data /var/www/storage /var/www/bootstrap/cache \
    && chmod -R 775 /var/www/storage /var/www/bootstrap/cache \
    && chmod +x /usr/local/bin/start.sh \
    && ln -sf /dev/stdout /var/www/storage/logs/laravel.log \
    && ln -sf /dev/stderr /var/www/storage/logs/laravel-error.log

USER www-data

HEALTHCHECK --interval=10s --timeout=5s --start-period=30s --retries=3 \
    CMD php-fpm -t && php artisan --version || exit 1

EXPOSE 9000

CMD ["/usr/local/bin/start.sh"]