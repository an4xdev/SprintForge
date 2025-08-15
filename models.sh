#!/bin/bash
cd js_express
./node_modules/.bin/sequelize-auto -o "./models" -d project -h localhost -u postgres -p 5432 -x "P@ssword123!" -e postgres -l ts --tables Companies TaskTypes
cd ..
cd laravel
export DB_HOST=localhost
export DB_PORT=5432
export DB_DATABASE=project
export DB_USERNAME=postgres
export DB_PASSWORD="P@ssword123!"
export DB_CONNECTION=pgsql
php artisan config:clear > /dev/null 2>&1
php artisan code:models
cd ..
