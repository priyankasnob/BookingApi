# Booking API — OpenAPI & SwaggerHub Documentation

## Overview

The **BookingApi** OpenAPI specification is exported from the local .NET 10 minimal API development server and hosted in SwaggerHub for contract visualization and documentation.

- **API Name:** `BookingApi`
- **Version:** `1.0.0`
- **Visibility:** Private
- **SwaggerHub URL:** `https://app.swaggerhub.com/apis/<YOUR_SWAGGERHUB_USERNAME>/BookingApi/1.0.0`
- **Local OpenAPI Snapshot:** `docs/openapi/booking-api-v1.json`
- **Live Local OpenAPI Endpoint (Development):** `GET http://localhost:5050/openapi/v1.json`

## Local Testing vs. Cloud Documentation

- **SwaggerHub** acts as your professional API contract reference and schema viewer.
- **Postman** is used for functional local execution against `http://localhost:5050`, including authentication (`/auth/login`), bearer tokens (`{{mayaAccessToken}}`), ownership validation (`404 Not Found`), and conflict prevention (`409 Conflict`).

## Updating the OpenAPI Specification

1. Start the API in development mode:
   ```bash
   ASPNETCORE_ENVIRONMENT=Development dotnet run --no-launch-profile -- --urls http://localhost:5050
