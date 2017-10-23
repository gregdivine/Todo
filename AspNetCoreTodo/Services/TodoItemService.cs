using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Data;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreTodo.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly ApplicationDbContext _context;

        public TodoItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddItemAsync(NewTodoItem newItem)
        {
            var entity = new TodoItem
            {
                Id = Guid.NewGuid(),
                IsDone = false,
                Title = newItem.Title,
                DueAt = DateTimeOffset.Now.AddDays(3)
            };

            _context.Items.Add(entity);
            var saveResult = await _context.SaveChangesAsync();

            return saveResult == 1;
        }

        public async Task<IEnumerable<TodoItem>> GetIncompleteItemsAsync()
        {
            var items = await _context.Items
                .Where(x => x.IsDone == false)
                .ToArrayAsync();

            return items;
        }

        public async Task<bool> MarkDoneAsync(Guid id)
        {
            var entity = await _context.Items
                .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null) return false;

            _context.Items.Update(entity);
            entity.IsDone = true;
            var saveResult = await _context.SaveChangesAsync();

            return saveResult == 1;
        }
    }
}
