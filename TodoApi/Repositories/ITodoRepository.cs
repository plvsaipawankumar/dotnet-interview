using TodoApi.Models;

namespace TodoApi.Repositories
{
    public interface ITodoRepository
    {
        Todo Create(Todo todo);
        Todo GetById(int id);
        List<Todo> GetAll();
        Todo Update(int id, Todo todo);
        bool Delete(int id);
    }
}
