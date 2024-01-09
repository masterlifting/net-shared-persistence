<!-- @format -->

# Net.Shared.Persistence

## Overview

The Persistence Providers Management Library is a comprehensive .NET library designed for managing various persistence providers such as MongoDB, PostgreSQL, AzureTable, and others. It features common interfaces for Repositories and Contexts, along with extensions methods for provider registration.

## Key Features

- **Unified Interfaces:** Offers standardized interfaces for Repositories and Contexts, facilitating consistency across different persistence providers.
- **Provider Registration Extensions:** Provides extension methods for easy registration of each persistence provider, ensuring a streamlined setup process.
- **Flexible Provider Management:** Supports the management of multiple providers, allowing one provider per host, with the flexibility to adapt to various database technologies.

## Usage

This library is essential for developers who work with multiple persistence providers in their .NET applications. It simplifies the integration and management of these providers through a unified approach, enhancing code maintainability and scalability.

## Integration and Configuration

To integrate this library:

1. Add it as a library or via Nuget package.
2. Use specific methods provided by the library to register your chosen provider with a context derived from the base context.
3. Implement the common repository interface methods in your application code.

The library includes detailed documentation with examples to guide through the integration and configuration process.

---

Leverage this library to efficiently manage various persistence providers in your .NET applications, ensuring robust, scalable, and maintainable data management solutions.

## NOTE: This library is still in development and is not yet ready for production use.

## NOTE: This library requires my specific dependencies. Look at the `.csproj` file for more information.
