# TODO API

Simple ASP.NET Core TODO API backed by SQLite.

## Running

```bash
dotnet run --project TodoApi
```

Swagger is available at `/swagger` when running in Development.

## Endpoints

- `GET /api/todos` - list TODO items
- `GET /api/todos/{id}` - retrieve one TODO item
- `POST /api/todos` - create a TODO item
- `PUT /api/todos/{id}` - update a TODO item
- `DELETE /api/todos/{id}` - delete a TODO item

## Testing

```bash
dotnet test
```
