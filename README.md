# IP Block API System (.NET 8)

## Overview
This is a RESTful Web API built using .NET 8 that manages IP geolocation and country-based blocking (permanent and temporary) using in-memory storage (no database).

---

## Features
-  IP Geolocation using external APIs  
-  Block countries permanently or temporarily  
-  Automatic expiration for temporary blocks (Background Service)  
-  Logging all blocked attempts  
-  Swagger API documentation for testing  

---

## Tech Stack
- .NET 8 Web API  
- C#  
- In-Memory Storage (ConcurrentDictionary)  
- Dependency Injection  
- HttpClientFactory  
- Background Services  
- Swagger  

---

## Architecture
Controllers → Services → Repositories → External APIs → Background Services

---

📖 API Testing

Open Swagger UI:
http://localhost:5020/swagger


## How to Run

```bash
git clone https://github.com/doaashazly-4/IpBlockApi.git
cd IpBlockApi
dotnet restore
dotnet build
dotnet run
