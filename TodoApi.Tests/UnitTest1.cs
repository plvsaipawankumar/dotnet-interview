using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.Controllers;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Services;

namespace TodoApi.Tests;

public class TodoServiceTests
{
    private readonly Mock<ITodoRepository> _repository = new();
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        _service = new TodoService(_repository.Object);
    }

    [Fact]
    public void CreateTodo_WithValidTodo_TrimsAndSavesTodo()
    {
        _repository.Setup(repository => repository.Create(It.IsAny<Todo>()))
            .Returns((Todo todo) => new Todo
            {
                Id = 1,
                Title = todo.Title,
                Description = todo.Description,
                CreatedAt = DateTime.UtcNow
            });

        var result = _service.CreateTodo(new Todo
        {
            Title = "  Buy milk  ",
            Description = "  Whole milk  "
        });

        Assert.Equal(1, result.Id);
        Assert.Equal("Buy milk", result.Title);
        Assert.Equal("Whole milk", result.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateTodo_WithMissingTitle_ThrowsValidationError(string title)
    {
        Assert.Throws<ArgumentException>(() => _service.CreateTodo(new Todo { Title = title }));
        _repository.Verify(repository => repository.Create(It.IsAny<Todo>()), Times.Never);
    }

    [Fact]
    public void GetAllTodos_ReturnsRepositoryResults()
    {
        _repository.Setup(repository => repository.GetAll()).Returns(new List<Todo>
        {
            new() { Id = 1, Title = "First" },
            new() { Id = 2, Title = "Second", IsCompleted = true }
        });

        var result = _service.GetAllTodos();

        Assert.Equal(2, result.Count);
        Assert.True(result[1].IsCompleted);
    }

    [Fact]
    public void GetTodoById_WithInvalidId_ThrowsValidationError()
    {
        Assert.Throws<ArgumentException>(() => _service.GetTodoById(0));
    }

    [Fact]
    public void UpdateTodo_WithMissingTodo_ReturnsNull()
    {
        _repository.Setup(repository => repository.Update(99, It.IsAny<Todo>()))
            .Returns((Todo?)null);

        var result = _service.UpdateTodo(99, new Todo { Title = "Missing" });

        Assert.Null(result);
    }

    [Fact]
    public void DeleteTodo_ReturnsRepositoryResult()
    {
        _repository.Setup(repository => repository.Delete(5)).Returns(true);

        var result = _service.DeleteTodo(5);

        Assert.True(result);
        _repository.Verify(repository => repository.Delete(5), Times.Once);
    }
}

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _service = new();
    private readonly TodoController _controller;

    public TodoControllerTests()
    {
        _controller = new TodoController(_service.Object, Mock.Of<ILogger<TodoController>>());
    }

    [Fact]
    public void GetAllTodos_ReturnsOkWithTodos()
    {
        _service.Setup(service => service.GetAllTodos()).Returns(new List<Todo>
        {
            CreateTodo(1, "First"),
            CreateTodo(2, "Second", isCompleted: true)
        });

        var result = _controller.GetAllTodos();

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<List<TodoDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count);
    }

    [Fact]
    public void GetTodoById_WhenTodoDoesNotExist_ReturnsNotFound()
    {
        _service.Setup(service => service.GetTodoById(42)).Returns((Todo?)null);

        var result = _controller.GetTodoById(42);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void CreateTodo_WithValidRequest_ReturnsCreatedTodo()
    {
        _service.Setup(service => service.CreateTodo(It.IsAny<Todo>()))
            .Returns((Todo todo) => CreateTodo(10, todo.Title, todo.Description));

        var result = _controller.CreateTodo(new CreateTodoRequest
        {
            Title = "New todo",
            Description = "Details"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ApiResponse<TodoDto>>(created.Value);
        Assert.Equal(nameof(TodoController.GetTodoById), created.ActionName);
        Assert.Equal(10, response.Data!.Id);
        Assert.Equal("New todo", response.Data.Title);
    }

    [Fact]
    public void CreateTodo_WithInvalidRequest_ReturnsBadRequest()
    {
        _service.Setup(service => service.CreateTodo(It.IsAny<Todo>()))
            .Throws(new ArgumentException("Title is required"));

        var result = _controller.CreateTodo(new CreateTodoRequest { Title = "" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void UpdateTodo_WhenTodoExists_ReturnsOk()
    {
        _service.Setup(service => service.UpdateTodo(3, It.IsAny<Todo>()))
            .Returns((int id, Todo todo) => CreateTodo(id, todo.Title, todo.Description, todo.IsCompleted));

        var result = _controller.UpdateTodo(3, new UpdateTodoRequest
        {
            Title = "Updated",
            Description = "Changed",
            IsCompleted = true
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<TodoDto>>(ok.Value);
        Assert.True(response.Data!.IsCompleted);
        Assert.Equal("Updated", response.Data.Title);
    }

    [Fact]
    public void DeleteTodo_WhenTodoDoesNotExist_ReturnsNotFound()
    {
        _service.Setup(service => service.DeleteTodo(8)).Returns(false);

        var result = _controller.DeleteTodo(8);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void UnexpectedServiceError_ReturnsInternalServerError()
    {
        _service.Setup(service => service.GetAllTodos())
            .Throws(new InvalidOperationException("database unavailable"));

        var result = _controller.GetAllTodos();

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
    }

    private static Todo CreateTodo(
        int id,
        string title,
        string? description = null,
        bool isCompleted = false)
    {
        return new Todo
        {
            Id = id,
            Title = title,
            Description = description,
            IsCompleted = isCompleted,
            CreatedAt = DateTime.UtcNow
        };
    }
}
