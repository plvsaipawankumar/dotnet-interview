using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        Todo CreateTodo(Todo todo);
        List<Todo> GetAllTodos();
        Todo? GetTodoById(int id);
        Todo? UpdateTodo(int id, Todo todo);
        bool DeleteTodo(int id);
    }

    public class TodoService : ITodoService
    {
        private readonly ITodoRepository _repository;

        public TodoService(ITodoRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public Todo CreateTodo(Todo todo)
        {
            if (todo == null)
                throw new ArgumentNullException(nameof(todo));

            ValidateTodo(todo);

            return _repository.Create(NormalizeTodo(todo));
        }

        public List<Todo> GetAllTodos()
        {
            return _repository.GetAll();
        }

        public Todo? GetTodoById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero", nameof(id));

            return _repository.GetById(id);
        }

        public Todo? UpdateTodo(int id, Todo todo)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero", nameof(id));

            if (todo == null)
                throw new ArgumentNullException(nameof(todo));

            ValidateTodo(todo);

            return _repository.Update(id, NormalizeTodo(todo));
        }

        public bool DeleteTodo(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero", nameof(id));

            return _repository.Delete(id);
        }

        private static void ValidateTodo(Todo todo)
        {
            if (string.IsNullOrWhiteSpace(todo.Title))
                throw new ArgumentException("Title is required", nameof(todo));

            if (todo.Title.Length > 200)
                throw new ArgumentException("Title cannot exceed 200 characters", nameof(todo));

            if (todo.Description?.Length > 1000)
                throw new ArgumentException("Description cannot exceed 1000 characters", nameof(todo));
        }

        private static Todo NormalizeTodo(Todo todo)
        {
            todo.Title = todo.Title.Trim();
            todo.Description = string.IsNullOrWhiteSpace(todo.Description)
                ? null
                : todo.Description.Trim();

            return todo;
        }
    }
}
