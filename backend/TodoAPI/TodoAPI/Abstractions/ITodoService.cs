using TodoAPI.DTOs.Todos;

namespace TodoAPI.Abstractions
{
    public interface ITodoService
    {
        Task<TodoDto?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<TodoDto>> GetTodosAsync(string? search, CancellationToken cancellationToken = default);
        Task<TodoDto> CreateTodoAsync(CreateTodoDto request, CancellationToken cancellationToken = default);
        Task<bool> UpdateTodoAsync(int id, UpdateTodoDto request, CancellationToken cancellationToken = default);
        Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default);
    }
}
