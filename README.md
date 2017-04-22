# DiscoveryApi
This is the repository of the Discovery API.
For now it doesn't do anything.

# What does it do
The primary objective of this API is to replace the old PHP model system that tends to be very unstable.

This API will provide:
- A list of online players
- The faction data
- The server statistics

Later on, the API will hopefully provide:
- Proper event data with archived scoreboards
- ???

# Technology
The API is ASP.NET Core based. 
The SQL connector is Pomelo.EntityFrameworkCore.MySql. You will have to update the connection string in appsettings.json