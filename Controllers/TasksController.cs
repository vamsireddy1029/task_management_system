using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.DTOs;
using TaskManagementApi.Models;
using FluentValidation;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IValidator<CreateTaskDto> _createValidator;
        private readonly IValidator<UpdateTaskDto> _updateValidator;
        private readonly IValidator<UpdateTaskStatusDto> _statusValidator;

        public TasksController(
            AppDbContext context,
            IValidator<CreateTaskDto> createValidator,
            IValidator<UpdateTaskDto> updateValidator,
            IValidator<UpdateTaskStatusDto> statusValidator)
        {
            _context = context;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _statusValidator = statusValidator;
        }

        // POST /api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            // Validate assigned user exists
            if (dto.AssignedToId.HasValue)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == dto.AssignedToId.Value))
                {
                    return BadRequest(new { error = "Assigned user does not exist" });
                }
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = (TaskItemStatus)dto.Status,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                AssignedToId = dto.AssignedToId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var taskDto = await GetTaskDto(task.Id);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskDto);
        }

        // GET /api/tasks
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TaskDto>>> GetTasks(
            [FromQuery] TaskItemStatus? status = null,
            [FromQuery] TaskPriority? priority = null,
            [FromQuery] int? assignedToId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.Tasks.AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedToId == assignedToId.Value);

            var totalCount = await query.CountAsync();

            var tasks = await query
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = (TaskStatus)t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    AssignedTo = t.AssignedTo != null ? new UserDto
                    {
                        Id = t.AssignedTo.Id,
                        Name = t.AssignedTo.Name,
                        Email = t.AssignedTo.Email
                    } : null
                })
                .ToListAsync();

            var result = new PaginatedResult<TaskDto>
            {
                Items = tasks,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(result);
        }

        // GET /api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            var taskDto = await GetTaskDto(id);

            if (taskDto == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            return Ok(taskDto);
        }

        // PUT /api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            // Validate assigned user exists
            if (dto.AssignedToId.HasValue)
            {
                if (!await _context.Users.AnyAsync(u => u.Id == dto.AssignedToId.Value))
                {
                    return BadRequest(new { error = "Assigned user does not exist" });
                }
            }

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = (TaskItemStatus)dto.Status;
            task.Priority = dto.Priority;
            task.DueDate = dto.DueDate;
            task.AssignedToId = dto.AssignedToId;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var taskDto = await GetTaskDto(id);

            return Ok(taskDto);
        }

        // PATCH /api/tasks/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<TaskDto>> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDto dto)
        {
            var validationResult = await _statusValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            task.Status = (TaskItemStatus)dto.Status;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var taskDto = await GetTaskDto(id);

            return Ok(taskDto);
        }

        // DELETE /api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound(new { error = "Task not found" });
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<TaskDto?> GetTaskDto(int id)
        {
            return await _context.Tasks
                .Include(t => t.AssignedTo)
                .Where(t => t.Id == id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = (TaskStatus)t.Status,
                    Priority = t.Priority,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    AssignedTo = t.AssignedTo != null ? new UserDto
                    {
                        Id = t.AssignedTo.Id,
                        Name = t.AssignedTo.Name,
                        Email = t.AssignedTo.Email
                    } : null
                })
                .FirstOrDefaultAsync();
        }
    }
}