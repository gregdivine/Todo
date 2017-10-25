using AspNetCoreTodo.Data;
using AspNetCoreTodo.Models;
using AspNetCoreTodo.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace AspNetCoreTodo.UnitTests
{
    public class TodoItemServiceTests
    {
        [Fact]
        public async Task AddItemAsync_AddsNewItem()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_AddNewItem").Options;

            // Set up context (connection to the DB) for writing
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(inMemoryContext);

                var fakeUser = new ApplicationUser
                {
                    Id = "fake-000",
                    UserName = "fake@fake"
                };

                await service.AddItemAsync(new NewTodoItem { Title = "AddNewItem" }, fakeUser);
            }

            // Use a separate context to read the data back from the DB
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                Assert.Equal(1, await inMemoryContext.Items.CountAsync());

                var item = await inMemoryContext.Items.FirstAsync();

                Assert.Equal("AddNewItem", item.Title);
                Assert.False(item.IsDone);
                Assert.True(DateTimeOffset.Now.AddDays(3) - item.DueAt < TimeSpan.FromSeconds(1));
            }
        }

        [Fact]
        public async Task MarkDoneAsync_ReturnsFalse_IfInputIncorrectItemId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_MarkDoneIncorrectId").Options;

            // Set up context (connection to the DB) for writing
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(inMemoryContext);

                var fakeUser = new ApplicationUser
                {
                    Id = "fake-001",
                    UserName = "fake@fake"
                };

                var incorrectId = Guid.NewGuid();

                await service.AddItemAsync(new NewTodoItem { Title = "MarkDoneIncorrectId" }, fakeUser);

                var result = await service.MarkDoneAsync(incorrectId, fakeUser);

                Assert.False(result);
            }
        }

        [Fact]
        public async Task MarkDoneAsync_ReturnsTrue_MarksItemAsCompleted_IfInputValidItemIdAndUser_()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_MarkDoneValidItemCompleted").Options;

            var fakeUser = new ApplicationUser
            {
                Id = "fake-002",
                UserName = "fake@fake"
            };

            // Set up context (connection to the DB) for writing
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(inMemoryContext);

                await service.AddItemAsync(new NewTodoItem { Title = "MarkDoneValidItemCompleted" }, fakeUser);
            }

            // Use a separate context to read the data back from the DB
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                Assert.Equal(1, await inMemoryContext.Items.CountAsync());

                var item = await inMemoryContext.Items.SingleAsync(i => i.OwnerId == fakeUser.Id);

                var service = new TodoItemService(inMemoryContext);

                var result = await service.MarkDoneAsync(item.Id, fakeUser);
                Assert.True(result);
            }

            // Use a separate context to read the data back from the DB
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                Assert.Equal(1, await inMemoryContext.Items.CountAsync());

                var item = await inMemoryContext.Items.SingleAsync(i => i.OwnerId == fakeUser.Id);
                Assert.True(item.IsDone);
            }
        }

        [Fact]
        public async Task GetIncompleteItemsAsync_ReturnsOnlyItemsOwnedByParticularUser()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "Test_GetIncompleteItemsAsync").Options;

            var user1 = new ApplicationUser
            {
                Id = "fake-003",
                UserName = "fake@fake"
            };

            var user2 = new ApplicationUser
            {
                Id = "fake-004",
                UserName = "fake@fake"
            };

            // Set up context (connection to the DB) for writing
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                var service = new TodoItemService(inMemoryContext);

                await service.AddItemAsync(new NewTodoItem { Title = "User1-1" }, user1);
                await service.AddItemAsync(new NewTodoItem { Title = "User1-2" }, user1);
                await service.AddItemAsync(new NewTodoItem { Title = "User2-1" }, user2);
                await service.AddItemAsync(new NewTodoItem { Title = "User2-2" }, user2);
            }

            // Use a separate context to read the data back from the DB
            using (var inMemoryContext = new ApplicationDbContext(options))
            {
                Assert.Equal(4, await inMemoryContext.Items.CountAsync());

                var service = new TodoItemService(inMemoryContext);

                var items = await service.GetIncompleteItemsAsync(user1);

                var result = items.OrderBy(i => i.Title).Select(i => i.Title).SequenceEqual(new[] { "User1-1", "User1-2" });

                Assert.True(result);
            }

        }
    }
}
