using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Services
{
    public interface ITodoService
    {
        Todo CreateTodo(Todo todo);
        List<Todo> GetAllTodos();
        Todo GetTodoById(int id);
        Todo UpdateTodo(int id, Todo todo);
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

            return _repository.Create(todo);
        }

        public List<Todo> GetAllTodos()
        {
            return _repository.GetAll();
        }

        public Todo GetTodoById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero", nameof(id));

            return _repository.GetById(id);
        }

        public Todo UpdateTodo(int id, Todo todo)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero", nameof(id));

            if (todo == null)
                throw new ArgumentNullException(nameof(todo));

            var existingTodo = _repository.GetById(id);
            if (existingTodo == null)
                throw new InvalidOperationException($"Todo with id {id} not found");

            return _repository.Update(id, todo);
        }

        public bool DeleteTodo(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero", nameof(id));

            var existingTodo = _repository.GetById(id);
            if (existingTodo == null)
                throw new InvalidOperationException($"Todo with id {id} not found");

            return _repository.Delete(id);
        }
    }
}
