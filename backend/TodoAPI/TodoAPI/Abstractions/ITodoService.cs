using TodoAPI.DTOs.Todos;

namespace TodoAPI.Abstractions
{
    public interface ITodoService
    {
        Task<TodoDto?> GetTodoByIdAsync(int userId, int id, CancellationToken cancellationToken = default);
        Task<List<TodoDto>> GetTodosAsync(int userId, string? search, CancellationToken cancellationToken = default);
        Task<TodoDto> CreateTodoAsync(int userId, CreateTodoDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateTodoAsync(int userId, int id, UpdateTodoDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteTodoAsync(int userId, int id, CancellationToken cancellationToken = default);
    }
}
