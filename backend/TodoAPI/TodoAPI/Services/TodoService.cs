using Microsoft.EntityFrameworkCore;
using TodoAPI.Abstractions;
using TodoAPI.Data;
using TodoAPI.DTOs.Todos;
using TodoAPI.Models;

namespace TodoAPI.Services
{
    public class TodoService : ITodoService
    {
        private readonly AppDbContext _context;

        public TodoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TodoDto> CreateTodoAsync(CreateTodoDto request, CancellationToken cancellationToken = default)
        {
            var existingTodo = await _context.Todos.AnyAsync(x => x.Title == request.Title, cancellationToken);
            if(existingTodo)
            {
                throw new InvalidOperationException("A todo with the same title already exists.");
            }

            var todo = new Todo
            {
                Title = request.Title,
                Description = request.Description,
                IsActive = request.IsActive,
                CreateDate = DateTime.UtcNow,
            };

            _context.Todos.Add(todo);
            await _context.SaveChangesAsync(cancellationToken);

            return new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsActive = todo.IsActive,
                CreateDate = todo.CreateDate
            };
        }

        public async Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if(todo is null)
            {
                return false;
            }

            _context.Todos.Remove(todo);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<TodoDto?> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Todos.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new TodoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDate = x.CreateDate
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<TodoDto>> GetTodosAsync(string? search, CancellationToken cancellationToken = default)
        {
            var query = _context.Todos.AsNoTracking().AsQueryable();

            if(!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => EF.Functions.ILike(x.Title, $"%{search.Trim()}%"));
            }
            return await query
            .OrderByDescending(x => x.CreateDate)
            .Select(x => new TodoDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDate = x.CreateDate
            })
            .ToListAsync(cancellationToken);
        }

        public async Task<bool> UpdateTodoAsync(int id, UpdateTodoDto request, CancellationToken cancellationToken = default)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if(todo is null)
            {
                return false;
            }
            
            var duplicateTodo = await _context.Todos.AnyAsync(x => x.Title == request.Title && x.Id != id, cancellationToken);
            if (duplicateTodo)
            {
                throw new InvalidOperationException("A todo with the same title already exists.");
            }

            todo.Title = request.Title;
            todo.Description = request.Description;
            todo.IsActive = request.IsActive;
            todo.CreateDate = request.CreateDate;
            todo.IsActive = request.IsActive;
            todo.UpdateDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
