# Restaurant Orders
## Initial Configuration
Update the [`appsettings.Development.json`](Restaurant%20Orders%2Fappsettings.Development.json)
or the [`appsettings.json`](Restaurant%20Orders%2Fappsettings.json) file(delete the [`appsettings.Development.json`](Restaurant%20Orders%2Fappsettings.Development.json) file in that case) to speficy the following values:
- Define the MSSQL database connection string in the `Database` property under the `ConnectionStrings` property.
- Define the Restaurant Owner or Admin name and email address in the properties under the `OwnerInfo` property.
- Change the JWT expiration duration in the `ExpireInMinutes` property under the `JWTInfo` property if necessary.
- Change the `DefaultPageSize` if necessary to alter the default page size used in pagination of data if no `pageSize` is provided with the request.
- Create the secrets:
  - Admin Password: `dotnet user-secrets set OwnerInfo:Password <password>`.
  - JWT Secret: `dotnet user-secrets set JWTInfo:Secret <a base64 encoded string>`. The base 64 encoded string can be generated from random bytes in "Git Bash" in windows with the command: `openssl rand -base64 32`.

## Database migration
Migrate the database using the command `dotnet ef database update`. If the dotnet-ef tool is not installed,
install the tool using the command `dotnet tool install --global dotnet-ef`.

## Run the project
Run the project using command: `dotnet run`. This will create the admin user automatically.

## Usages
- Login as restaurant owner(admin)/customer using the admin email and password(see swagger doc).
- Register new customers(see swagger doc).
- Find other API endpoints in swagger doc.
