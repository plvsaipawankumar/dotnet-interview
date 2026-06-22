using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/todos")]
    public class TodoController : ControllerBase
    {
        private readonly ITodoService _todoService;
        private readonly ILogger<TodoController> _logger;

        public TodoController(ITodoService todoService, ILogger<TodoController> logger)
        {
            _todoService = todoService ?? throw new ArgumentNullException(nameof(todoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all todo items
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllTodos()
        {
            try
            {
                var todos = _todoService.GetAllTodos();
                var todoDtos = todos.Select(MapTodoToDto).ToList();
                return Ok(ApiResponse<List<TodoDto>>.SuccessResponse(todoDtos, "Todos retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving todos");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResponse("An error occurred while retrieving todos"));
            }
        }

        /// <summary>
        /// Get a specific todo item by id
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetTodoById(int id)
        {
            try
            {
                var todo = _todoService.GetTodoById(id);
                if (todo == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse($"Todo with id {id} not found"));
                }

                return Ok(ApiResponse<TodoDto>.SuccessResponse(MapTodoToDto(todo), "Todo retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving todo with id {TodoId}", id);
                return HandleException(ex, "An error occurred while retrieving the todo");
            }
        }

        /// <summary>
        /// Create a new todo item
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateTodo([FromBody] CreateTodoRequest request)
        {
            try
            {
                var todo = new Todo
                {
                    Title = request?.Title ?? string.Empty,
                    Description = request?.Description,
                    IsCompleted = false
                };

                var createdTodo = _todoService.CreateTodo(todo);
                var todoDto = MapTodoToDto(createdTodo);

                return CreatedAtAction(nameof(GetTodoById), new { id = createdTodo.Id },
                    ApiResponse<TodoDto>.SuccessResponse(todoDto, "Todo created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating todo");
                return HandleException(ex, "An error occurred while creating the todo");
            }
        }

        /// <summary>
        /// Update an existing todo item
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
        {
            try
            {
                var todo = new Todo
                {
                    Title = request?.Title ?? string.Empty,
                    Description = request?.Description,
                    IsCompleted = request?.IsCompleted ?? false
                };

                var updatedTodo = _todoService.UpdateTodo(id, todo);
                if (updatedTodo == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse($"Todo with id {id} not found"));
                }

                return Ok(ApiResponse<TodoDto>.SuccessResponse(MapTodoToDto(updatedTodo), "Todo updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating todo with id {TodoId}", id);
                return HandleException(ex, "An error occurred while updating the todo");
            }
        }

        /// <summary>
        /// Delete a todo item
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteTodo(int id)
        {
            try
            {
                var result = _todoService.DeleteTodo(id);
                if (result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Todo deleted successfully"));
                }

                return NotFound(ApiResponse<object>.ErrorResponse("Failed to delete todo"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting todo with id {TodoId}", id);
                return HandleException(ex, "An error occurred while deleting the todo");
            }
        }

        private ObjectResult HandleException(Exception exception, string fallbackMessage)
        {
            if (exception is ArgumentException || exception is ArgumentNullException)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(exception.Message));
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse(fallbackMessage));
        }

        private static TodoDto MapTodoToDto(Todo todo)
        {
            return new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt
            };
        }
    }
}
