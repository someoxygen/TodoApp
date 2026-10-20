using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Abstractions;
using TodoAPI.DTOs.Todos;

namespace TodoAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;

        public TodosController(ITodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TodoDto>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var todos = await _todoService.GetTodosAsync(userId, search, cancellationToken);
            return Ok(todos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TodoDto>> GetById(int id, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var todo = await _todoService.GetTodoByIdAsync(userId, id, cancellationToken);
            if (todo is null)
            {
                return NotFound();
            }
            return Ok(todo);
        }

        [HttpPost]
        public async Task<ActionResult<TodoDto>> Create(CreateTodoDto request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var todo = await _todoService.CreateTodoAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateTodoDto request, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var updated = await _todoService.UpdateTodoAsync(userId, id, request, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var deleted = await _todoService.DeleteTodoAsync(userId, id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }
            return Ok();
        }
        private bool TryGetUserId(out int userId)
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return int.TryParse(claim, out userId) && userId > 0;
        }
    }
}
