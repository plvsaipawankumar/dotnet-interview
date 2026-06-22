using TodoApi.DTOs;

namespace TodoApi.Validators
{
    public static class TodoValidator
    {
        public static (bool IsValid, string ErrorMessage) ValidateCreateRequest(CreateTodoRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (string.IsNullOrWhiteSpace(request.Title))
                return (false, "Title is required");

            if (request.Title.Length > 200)
                return (false, "Title cannot exceed 200 characters");

            if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 1000)
                return (false, "Description cannot exceed 1000 characters");

            return (true, string.Empty);
        }

        public static (bool IsValid, string ErrorMessage) ValidateUpdateRequest(UpdateTodoRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (string.IsNullOrWhiteSpace(request.Title))
                return (false, "Title is required");

            if (request.Title.Length > 200)
                return (false, "Title cannot exceed 200 characters");

            if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 1000)
                return (false, "Description cannot exceed 1000 characters");

            return (true, string.Empty);
        }
    }
}
