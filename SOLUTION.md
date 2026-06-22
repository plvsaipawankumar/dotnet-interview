# Solution Documentation

**Candidate Name:** [Your Name]  
**Completion Date:** 2026-06-23

---

## Problems Identified

The original implementation had multiple architectural and security issues:
- All API operations were exposed through non-standard POST endpoints such as `/api/createTodo`, `/api/getTodo`, `/api/updateTodo`, and `/api/deleteTodo`.
- Business logic, validation, and data access were mixed inside the controller and service implementations.
- The application used raw SQL string interpolation in the data layer, which introduced a SQL injection risk.
- There was no clear separation between domain models and API contracts.
- The existing test project was incomplete and did not verify actual behavior.
- Error handling was inconsistent and did not provide stable response shapes or proper HTTP status codes.

---

## Architectural Decisions

This refactor uses a clean layered architecture with the following structure:
- `TodoApi/Controllers` for HTTP handling and request/response transformation.
- `TodoApi/Services` for business rules, input validation, and normalization.
- `TodoApi/Repositories` for data access and SQL persistence.
- `TodoApi/DTOs` for request/response contracts and consistent API payloads.

Key architectural patterns:
- Repository Pattern to isolate database access behind `ITodoRepository`.
- Dependency Injection to wire `ITodoRepository` and `ITodoService` into controllers cleanly.
- RESTful API design using HTTP verbs and resource-oriented routes (`/api/todos`).
- Explicit request validation in `TodoService` and controller-level exception handling.

This structure improves maintainability by isolating responsibilities and making each layer easier to test.

---

## Trade-offs

- Prioritized application design, security, and testability over adding advanced features.
- Kept the persistence layer simple with SQLite and hand-written SQL rather than introducing an ORM.
- Used an in-process repository implementation instead of a separate persistence service to stay within the scope of the exercise.
- Avoided over-engineering the response wrapper while still providing consistent API output through `ApiResponse<T>`.

---

## How to Run

### Prerequisites
- .NET 10 SDK installed
- Windows, macOS, or Linux
- A terminal or PowerShell session

### Build
```powershell
cd D:\workspace\C#\TODO\dotnet-interview
C:\Program Files\dotnet\dotnet.exe build
```

### Run
```powershell
C:\Program Files\dotnet\dotnet.exe run --project TodoApi\TodoApi.csproj
```

The API will start and use `Data Source=todos.db` by default.

### Test
```powershell
C:\Program Files\dotnet\dotnet.exe test
```

---

## API Documentation

Base URL: `https://localhost:5001` or `http://localhost:5000`

### Create TODO
```
Method: POST
URL: /api/todos
Request Body:
{
  "title": "Buy groceries",
  "description": "Milk, eggs, and bread"
}
Response:
{
  "success": true,
  "message": "Todo created successfully",
  "data": {
    "id": 1,
    "title": "Buy groceries",
    "description": "Milk, eggs, and bread",
    "isCompleted": false,
    "createdAt": "2026-06-23T12:34:56Z"
  }
}
```

### Get All TODOs
```
Method: GET
URL: /api/todos
Response:
{
  "success": true,
  "message": "Todos retrieved successfully",
  "data": [
    {
      "id": 1,
      "title": "Buy groceries",
      "description": "Milk, eggs, and bread",
      "isCompleted": false,
      "createdAt": "2026-06-23T12:34:56Z"
    }
  ]
}
```

### Get TODO by Id
```
Method: GET
URL: /api/todos/{id}
Response:
{
  "success": true,
  "message": "Todo retrieved successfully",
  "data": {
    "id": 1,
    "title": "Buy groceries",
    "description": "Milk, eggs, and bread",
    "isCompleted": false,
    "createdAt": "2026-06-23T12:34:56Z"
  }
}
```

### Update TODO
```
Method: PUT
URL: /api/todos/{id}
Request Body:
{
  "title": "Buy groceries and snacks",
  "description": "Milk, eggs, bread, chips",
  "isCompleted": true
}
Response:
{
  "success": true,
  "message": "Todo updated successfully",
  "data": {
    "id": 1,
    "title": "Buy groceries and snacks",
    "description": "Milk, eggs, bread, chips",
    "isCompleted": true,
    "createdAt": "2026-06-23T12:34:56Z"
  }
}
```

### Delete TODO
```
Method: DELETE
URL: /api/todos/{id}
Response:
{
  "success": true,
  "message": "Todo deleted successfully",
  "data": null
}
```

### Error Responses
- `400 Bad Request` for validation failures or invalid IDs
- `404 Not Found` when a TODO item does not exist
- `500 Internal Server Error` for unexpected failures

---

## Testing Strategy

- Added unit tests for controller and service behavior.
- Used `Moq` to isolate dependencies and verify interactions.
- Covered positive and negative paths, including validation, error handling, and not-found behavior.
- Tests are organized by component and endpoint behavior.

---

## Security Improvements

- Replaced raw SQL interpolation with parameterized queries in `TodoRepository`.
- Added input validation to prevent invalid title and description values.
- Centralized exception mapping so invalid requests return `400` and unexpected errors return `500`.
- Used dependency injection for better testability and safer configuration.

---

## Future Improvements

- Add a database migration or schema management layer.
- Introduce DTO validation attributes and automatic model binding validation.
- Add integration tests against the SQLite database.
- Add authentication and authorization for user-specific TODOs.
- Use a richer logging format and request correlation IDs.
- Replace custom response wrapper with proper HATEOAS or more REST-native contracts if needed.
