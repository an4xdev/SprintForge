cd js_express
.\node_modules\.bin\sequelize-auto -o "./models" -d project -h localhost -u postgres -p 5432 -x P@ssword123! -e postgres -l ts --tables Companies TaskTypes
cd ..
cd laravel
set DB_HOST=localhost
set DB_PORT=5432
set DB_DATABASE=project
set DB_USERNAME=postgres
set DB_PASSWORD=P@ssword123!
set DB_CONNECTION=pgsql
php artisan config:clear >nul 2>&1
php artisan code:models
cd ..
cd go\generator
go run .
cd .. 
cd ..