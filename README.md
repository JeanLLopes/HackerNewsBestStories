# Hacker News Best Stories API

This project implements a RESTful API in ASP.NET Core that fulfills the provided coding challenge. The API retrieves the best stories from the public Hacker News API, as determined by their score.

The primary focus of this project is **efficiency** and **resilience**, ensuring the API can handle a large number of requests without overloading the external Hacker News API. This is achieved through a distributed caching strategy (using Redis) and parallel processing.

## Features

* Retrieves the IDs of the "best stories" (`beststories.json`).
* Fetches the details for each story (`item/{id}.json`) in parallel.
* Caches the full list of processed stories for a short period to serve subsequent requests instantly.
* Returns a sorted list of the `n` best stories in the specified JSON format.
## How to Run the Application

### Prerequisites

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or the version used in the project, e.g., 6.0, 7.0)
* Visual Studio 2022 or a code editor like VS Code.

### Instructions

#### **Run with Docker Compose (Recommended for Redis integration):**

For those who want to run the API with Redis integration using Docker, a `docker-compose.yml` file is provided. This is the recommended approach to easily spin up the required services.

Make sure you have [Docker](https://www.docker.com/get-started) and [Docker Compose](https://docs.docker.com/compose/) installed.

* **Steps:**
    1. Navigate to the project directory (where the `docker-compose.yml` is located).
    2. Run the following command to start the services:
        ```bash
        docker-compose up --build
        ```
    3. Once the services are up, the API should be accessible as per the Docker configuration (usually `http://localhost:5000` or `https://localhost:5001`).

    * The API will be available at `http://localhost:8099`.
    * Redis will be available at `localhost:6379` (internal to Docker network).

#### 3. **Run the application (via CLI, without Docker):**
* Make sure you have a Redis server running and update the connection string in `appsettings.json`:
    ```json
    {
      "ConnectionStrings": {
        "Redis": "localhost:6379"
      }
    }
    ```
* Navigate to the API project directory:
    ```bash
    cd HackerNewsBestStories.Api
    ```
* Restore dependencies and run the project:
    ```bash
    dotnet restore
    dotnet run
    ```

#### 4. **Run the application (via Visual Studio):**
* Open the `.sln` (Solution) file in Visual Studio.
* Ensure `HackerNewsBestStories.Api` is set as the "Startup Project".
* Press **F5** or the "Play" button to start the application.

## How to Use the API

Once the application is running, it will host a Swagger (OpenAPI) interface for easy viewing and testing of the endpoints.

* **Swagger URL:** `http://localhost:8099/swagger`
* **API URL (example):** `http://localhost:8099/api/v1/stories/best`

### Endpoint

#### GetBestStories

Returns the  best stories from Hacker News.

* **Method:** `GET`
* **Endpoint:** `/api/v1/stories/best`
* **Query Parameter:**
    * `n` (integer, optional, default: 10): The number of stories to return.

* **Example Request:**
    `GET http://localhost:8099/api/v1/stories/best?n=20`

* **Example Response (`200 OK`):**
    ```json
    [
      {
        "title": "A uBlock Origin update was rejected from the Chrome Web Store",
        "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
        "postedBy": "ismaildonmez",
        "time": "2019-10-12T13:43:01+00:00",
        "score": 1716,
        "commentCount": 572
      },
      {
        "title": "Another Story Title",
        "uri": "https://example.com/another-story",
        "postedBy": "another_user",
        "time": "2023-10-18T20:00:00+00:00",
        "score": 1500,
        "commentCount": 300
      }
    ]
    ```

## Design Decisions and Assumptions

1.  **Field Mapping:** To match the requested output format, the following mappings from the Hacker News API were assumed:
    * `"uri"` in the response was mapped from the `"url"` field.
    * `"postedBy"` in the response was mapped from the `"by"` field.
    * `"time"` in the response (ISO 8601 string) was converted from the `"time"` field (Unix timestamp).
    * `"commentCount"` in the response was mapped from the `"descendants"` field.

2.  **Efficiency Strategy:** To meet the requirement to "efficiently service large numbers of requests", a distributed cache (Redis) strategy was implemented. The full list of best stories is fetched from the HN API, processed, sorted, and stored in Redis. User requests are served from this cache, which expires after a short period (e.g., 2 minutes), ensuring the HN API is not overloaded by repeated requests and allowing multiple API instances to share the same cache.

3.  **Item Fetch Failures:** If fetching an individual story's details (e.g., `v0/item/{id}.json`) fails, that item is simply omitted from the final list rather than failing the entire request.


## Docker Compose

A `docker-compose.yml` is provided to run both the API and Redis together for local development and testing.

---

**Note:**  
If you scale the API horizontally (multiple containers/instances), all will share the same Redis cache, ensuring consistent and efficient responses.

## Running Tests

The project includes a comprehensive test suite to ensure reliability and correctness. To run the tests:

### Using Visual Studio:
1. Open the solution in Visual Studio
2. Open Test Explorer (__Test > Test Explorer__)
3. Click "Run All Tests" or use the shortcut (Ctrl+R, A)

### Using Command Line:

The test suite includes unit tests for components such as the StoryValidator service to ensure proper validation of story data.