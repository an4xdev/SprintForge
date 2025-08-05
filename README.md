# SprintForge

Software project management system implemented in microservices on different programming platforms 

## Used technologies

- ASP.NET Core (MIT/Apache 2.0)
- Spring Boot (Apache 2.0)
- FastAPI (MIT)
- Express.js (MIT)
- Laravel (MIT)
- Vue.js (MIT)
- PostgreSQL (PostgreSQL License)
- Redis (BSD 3-Clause)
- RabbitMQ (MPL 2.0)
- MinIO (AGPL 3.0)

## Status

**IN DEVELOPMENT**

## Quick start

```console
docker-compose up --build -d
```

Api Gateway OpenAPI documentation is available at [here](https://localhost/scalar/v1).

## Development Tips

### HTTPS Api Gateway

Follow [Microsoft documentation](https://learn.microsoft.com/en-us/aspnet/core/security/docker-compose-https?view=aspnetcore-9.0)


### Automatic migrations

Migrations are managed by `DatabaseService` which is using objects from `SharedObjects` project. It is checking for the latest migration and applying it to the database.

### Automatic model generation from database

> NOTE  
> For Spring project I used Intellij IDEA option from Database tab.

> NOTE  
> For FastAPI project I hand wrote models based on the database schema. But got inspired by models that were generated via [sqlacodegen](https://pypi.org/project/sqlacodegen/).

#### Requirements

- Laravel: [Reliese laravel](https://github.com/reliese/laravel)
- Express: [sequelize-auto](https://github.com/sequelize/sequelize-auto) with PostgreSQL dialect

#### Usage

To automatically generate models from database, you can use the following command:

```bash
# For Windows
models.bat
# For Linux !!! NOT TESTED !!!
chmod +x models.sh
./models.sh
```

## Signing Token for JWT
To generate a signing token for JWT, you can use the following command (or leave my token in the `.env` file):

```bash
cd key_generator
python main.py
```

Modify the `.env` file with newly generated token.

## Licenses

Third party libraries are licensed under their respective licenses see third-party-licenses directory for details.