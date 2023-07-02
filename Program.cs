using Microsoft.EntityFrameworkCore;
using TodoApi;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("myPolicy",
                        policy =>
                        {
                            policy.AllowAnyOrigin()
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                        });
});
var app = builder.Build();

app.UseCors("myPolicy");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

app.MapGet("/todoitems", (ToDoDbContext context) =>
{
   return context.Items;
});

app.MapPost("/todoitems", async (ToDoDbContext context, Item item) =>
{
    if (context.Items.Count() > 0)
        item.Id = await context.Items.MaxAsync(i => i.Id) + 1;
    else
        item.Id = 1;
    context.Items.Add(item);
    await context.SaveChangesAsync();
    return item;
});

app.MapPut("/todoitems/{id}", async (ToDoDbContext context, Item item, int id) =>
{
    var todoItem = await context.Items.FindAsync(id);
    if (todoItem is null) return Results.NotFound();
    todoItem.IsComplete = item.IsComplete;
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (ToDoDbContext context, int id) =>
{
    if (await context.Items.FindAsync(id) is Item item)
    {
        context.Items.Remove(item);
        await context.SaveChangesAsync();
        return Results.Ok(item);
    }
    return Results.NotFound();
});

app.Run();
