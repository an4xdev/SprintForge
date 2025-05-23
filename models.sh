#!/bin/bash

cd js_express
./node_modules/.bin/sequelize-auto -o "./models" -d project -h localhost -u postgres -p 5432 -x "P@ssword123!" -e postgres -l ts # propably can use just sequelize-auto

cd ..

cd laravel
php artisan code:models
