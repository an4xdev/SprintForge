cd js_express
.\node_modules\.bin\sequelize-auto -o "./models" -d project -h localhost -u postgres -p 5432 -x P@ssword123! -e postgres -l ts --tables Companies TaskTypes

cd ..

cd laravel
php artisan code:models