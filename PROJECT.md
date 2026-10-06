# PaymentTrackerApi — API Architecture

## Purpose
ASP.NET Core payment-tracking API.

## Components
Controllers, DTOs, Models, Services, Data, Enums and EF Core Migrations form the main application layers. Program.cs configures the application and dependency injection.

## AI Workflow
Trace request -> controller -> DTO/service -> data/model -> response before changing behavior.

## Rules
Preserve financial/payment data integrity, validation and idempotency. Never log secrets or sensitive payment data. Validate database migrations and affected API endpoints.