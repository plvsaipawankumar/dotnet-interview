using Microsoft.Data.Sqlite;
using TodoApi.Models;

namespace TodoApi.Repositories
{
    public class TodoRepository : ITodoRepository
    {
        private readonly string _connectionString;

        public TodoRepository(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
        }

        public Todo Create(Todo todo)
        {
            if (todo == null)
                throw new ArgumentNullException(nameof(todo));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            var createdAt = DateTime.UtcNow;
            command.CommandText = @"
                INSERT INTO Todos (Title, Description, IsCompleted, CreatedAt)
                VALUES (@title, @description, @isCompleted, @createdAt);
                SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("@title", todo.Title ?? string.Empty);
            command.Parameters.AddWithValue("@description", (object?)todo.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@isCompleted", todo.IsCompleted ? 1 : 0);
            command.Parameters.AddWithValue("@createdAt", createdAt.ToString("o"));

            var id = Convert.ToInt32(command.ExecuteScalar());
            todo.Id = id;
            todo.CreatedAt = createdAt;
            return todo;
        }

        public List<Todo> GetAll()
        {
            var todos = new List<Todo>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                todos.Add(MapReaderToTodo(reader));
            }

            return todos;
        }

        public Todo? GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapReaderToTodo(reader);
            }

            return null;
        }

        public Todo? Update(int id, Todo todo)
        {
            if (todo == null)
                throw new ArgumentNullException(nameof(todo));

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Todos
                SET Title = @title, Description = @description, IsCompleted = @isCompleted
                WHERE Id = @id
            ";

            command.Parameters.AddWithValue("@title", todo.Title ?? string.Empty);
            command.Parameters.AddWithValue("@description", (object?)todo.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@isCompleted", todo.IsCompleted ? 1 : 0);
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = command.ExecuteNonQuery();
            if (rowsAffected == 0)
                return null;

            return GetById(id);
        }

        public bool Delete(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Todos WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            var rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        private static Todo MapReaderToTodo(SqliteDataReader reader)
        {
            return new Todo
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                IsCompleted = reader.GetInt32(3) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind)
            };
        }
    }
}
