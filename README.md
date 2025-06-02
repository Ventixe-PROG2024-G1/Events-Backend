# Events Service Provider

---

### Get Events Sequence Diagram

![NPPLK-~1](https://github.com/user-attachments/assets/befb5b07-703e-4194-b1d6-82efce3619d9)


---

### Creating Events Sequence Diagram

![HLLHJZ~1](https://github.com/user-attachments/assets/5938879b-b68f-444f-a2fe-1c1fbeef7923)


---

# Events Web API

Events Web API is a comprehensive .NET 9-based RESTful service designed for managing events and their associated categories. This API enables you to create, retrieve, update, and delete events while providing advanced search, filtering, and pagination capabilities.

##  Features

###  Core Functionality
*   **Complete Event Management:** Create, read, update, and delete events through dedicated API endpoints.
*   **Category System:** Events are logically organized into predefined categories (e.g., Music, Technology, Sports), allowing for structured event browsing and filtering.
*   **Advanced Querying:**
    *   Filter events by **category name**.
    *   Filter events by **date ranges**, supporting specific dates or predefined periods like "thisweek", "upcoming", "past".
    *   Utilize **search terms** to query across event names, descriptions, and category names.
    *   Filter events by their **status** (e.g., Draft, Active, Past, Cancelled).
*   **Pagination:** Efficiently handle large datasets with built-in pagination for event listings.

###  Technical Features
*   **Multi-level Caching:** Implements an intelligent caching strategy:
    *   Caches individual event details for quick retrieval.
    *   Caches collections of events (e.g., paginated lists, filtered results).
    *   Ensures cache invalidation on event creation, updates, or deletion to maintain data consistency.
*   **API Security:** Protects endpoints using API Key authentication.
*   **Swagger Documentation:** Provides comprehensive and interactive API documentation through Swagger (OpenAPI), including example requests and responses.
*   **CORS Support:** Offers configurable Cross-Origin Resource Sharing (CORS) to allow requests from different domains.

##  Technical Stack

*   **Framework:** .NET 9
*   **Language:** C# 13
*   **ORM:** Entity Framework Core 9.0.5
*   **Database:** SQL Server
*   **Caching:** In-memory caching
*   **Mapping:** Mapperly for efficient object-to-object mapping
*   **API Documentation:** Swagger/OpenAPI

##  Architecture

The project follows a clean, layered architecture to promote separation of concerns and maintainability:

1.  **Controllers Layer:** (`Presentation`) Handles incoming HTTP requests, validates input, and orchestrates responses. Interacts with the Services Layer.
2.  **Services Layer:** (`Application/Business Logic`) Contains the core business logic, orchestrates operations between controllers and repositories, and handles data transformations.
3.  **Repository Layer:** (`Infrastructure/Persistence`) Manages data access and persistence logic, abstracting the data store (SQL Server via EF Core) from the rest of the application.
4.  **Data Layer/Domain:** (`Domain/Entities`) Defines the core entities (e.g., Event, Category) and their relationships using Entity Framework Core.

### Key Components:
*   `EventController`: Exposes RESTful endpoints for all event-related operations.
*   `EventService`: Implements the business logic for managing events and categories.
*   `EventRepository`: Provides data access methods for CRUD operations on events and categories.
*   `CacheHandler` (or similar): A generic component or service responsible for caching logic.
*   `ApiKeyMiddleware`: Custom middleware for handling API Key authentication.

##  Data Model

The API manages the following core entities:

### Event
*   **Id:** Unique identifier (e.g., `Guid` or `int`)
*   **Name:** `string` - The title of the event.
*   **Description:** `string` - A detailed description of the event.
*   **StartDate:** `DateTime` - The starting date and time of the event.
*   **EndDate:** `DateTime` - The ending date and time of the event.
*   **CategoryId:** Identifier linking to a `Category`.
*   **Category:** Navigation property to the associated `Category`.
*   **Status:** `enum` or `string` - Represents the current state of the event (e.g., Draft, Active, Past, Cancelled).
*   **ImageUrl:** `string` (optional) - URL to an image representing the event.
*   **Location:** `string` (optional) - Physical or virtual location of the event.

### Category
*   **Id:** Unique identifier (e.g., `Guid` or `int`)
*   **Name:** `string` - The name of the category.
*   **Events:** Navigation property to a collection of `Event` entities belonging to this category.

##  API Endpoints

All endpoints are typically prefixed with `/api`.

### Events
*   **`GET /Event/all`**
    *   Description: Retrieve all events (consider if this should be paginated by default or removed if `/Event` serves the same purpose with pagination).
    *   Response: `200 OK` with a list of all event objects.
*   **`GET /Event`**
    *   Description: Get a paginated list of events with support for filtering.
    *   Query Parameters:
        *   `categoryName` (string): Filter by category name.
        *   `dateRange` (string): e.g., "thisweek", "upcoming", "past", or a specific date "YYYY-MM-DD".
        *   `startDate` (DateTime): Filter events starting on or after this date.
        *   `endDate` (DateTime): Filter events ending on or before this date.
        *   `search` (string): Search term for event name, description, category.
        *   `status` (string): Filter by event status (Draft, Active, Past, Cancelled).
        *   `pageNumber` (int): The page number for pagination (default: 1).
        *   `pageSize` (int): The number of items per page (default: 10).
    *   Response: `200 OK` with a paginated list of event objects.
*   **`GET /Event/{id}`**
    *   Description: Get a specific event by its unique identifier.
    *   Path Parameter: `id` - The ID of the event.
    *   Response: `200 OK` with the event object, or `404 Not Found`.
*   **`POST /Event`**
    *   Description: Create a new event.
    *   Request Body: JSON object representing the event to be created.
    *   Response: `201 Created` with the created event object and a `Location` header, or `400 Bad Request` if validation fails.
*   **`PUT /Event/{id}`**
    *   Description: Update an existing event.
    *   Path Parameter: `id` - The ID of the event to update.
    *   Request Body: JSON object with the fields to be updated.
    *   Response: `200 OK` with the updated event object, `204 No Content` on successful update with no body, `404 Not Found`, or `400 Bad Request`.
*   **`DELETE /Event/{id}`**
    *   Description: Delete an event.
    *   Path Parameter: `id` - The ID of the event to delete.
    *   Response: `204 No Content` on successful deletion, or `404 Not Found`.

### Categories
*   **`GET /Category`**
    *   Description: Retrieve all available event categories.
    *   Response: `200 OK` with a list of category objects.

##  Authentication

The API uses an API Key-based authentication scheme. To access the protected endpoints, a valid API key must be included in the `X-API-KEY` HTTP header with each request.

**Example Header:**
`X-API-KEY: your_secret_api_key_here`

Requests without a valid API key or with an incorrect key will result in a `401 Unauthorized` response.

##  Getting Started

### Prerequisites
*   [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
*   SQL Server (e.g., SQL Server Express, Developer Edition, or a cloud instance)
*   A code editor like Visual Studio 2022+ or VS Code.

### Configuration
1.  **Clone the repository:**
    ```bash
    git clone <YOUR_REPOSITORY_URL>
    cd <PROJECT_DIRECTORY_NAME>
    ```
2.  **Set up Connection String:**
    Configure your SQL Server connection string in `appsettings.Development.json` (for local development) or `appsettings.json`. For production, use environment variables or Azure Key Vault.
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=EventsWebApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
      },
      "ApiSettings": {
        "ApiKey": "YOUR_SUPER_SECRET_API_KEY" // Define the API key the service will expect
      }
    }
    ```
    *Update `DefaultConnection` to point to your SQL Server instance.*
    *Set a strong `ApiKey` for `ApiSettings:ApiKey`.*

3.  **Database Migrations:**
    If using Entity Framework Core migrations (Code First):
    ```bash
    # Navigate to the project directory containing the DbContext
    dotnet ef database update
    ```
    This will create the database and schema if they don't exist.

### Running the API
*   **Using Visual Studio:**
    1.  Open the solution (`.sln`) file.
    2.  Set the API project as the startup project.
    3.  Press `F5` or click the "Start" button.
*   **Using .NET CLI:**
    Navigate to the API project directory and run:
    ```bash
    dotnet run
    ```
The API will typically be available at `https://localhost:<port>` or `http://localhost:<port>`. The Swagger UI will usually be accessible at `/swagger`. Check your `launchSettings.json` for the exact ports.
