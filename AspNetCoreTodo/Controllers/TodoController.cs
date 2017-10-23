using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreTodo.Services;
using AspNetCoreTodo.Models;

namespace AspNetCoreTodo.Controllers
{
    public class TodoController : Controller
    {
        private readonly ITodoItemService _todoItemService;

        public TodoController(ITodoItemService todoItemService)
        {
            _todoItemService = todoItemService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Get to-do items from database
            var todoItems = await _todoItemService.GetIncompleteItemsAsync();

            // Put items into a model
            var model = new TodoViewModel
            {
                Items = todoItems
            };

            // Pass the view to a model and render

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(NewTodoItem newItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var successful = await _todoItemService.AddItemAsync(newItem);

            if (!successful)
            {
                return BadRequest(new { error = "Could not add item"});
            }

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var successful = await _todoItemService.MarkDoneAsync(id);

            if (!successful)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}