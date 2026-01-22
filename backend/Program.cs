using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
    });
});
builder.Services.AddSqlite<AppDbContext>("Data Source=promanage_plus.db");

var app = builder.Build();

app.UseCors();


app.MapGet("/tasks", async (AppDbContext db) =>
{
    return await db.Tasks
                    .Include(t => t.Category)
                    .ToListAsync();
});

app.MapPost("/tasks", async (AppDbContext db, TaskItem task) =>
{
    var CategoryExists = await db.Categories.AnyAsync(c => c.Id == task.CategoryId);
    if (!CategoryExists)
    {
        return Results.BadRequest("指定されたカテゴリーは存在しません");
    }

    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapGet("/categories/{id}/tasks", async (AppDbContext db, int id) =>
{
    var category = await db.Categories
                            .Include(c => c.Tasks)
                            .FirstOrDefaultAsync(c => c.Id == id);

    if (category is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new
    {
        CategoryName = category.Name,
        Tasks = category.Tasks
    });
});

app.Run();


public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Color { get; set; }

    public List<TaskItem> Tasks { get; set; } = new ();
}

public class TaskItem
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public double Hours { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int CategoryId { get; set; }

    [JsonIgnore] // JSONに変換するとき、ここを無視してループを止める
    public Category? Category { get; set; }
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {  }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.CategoryId);

        modelBuilder.Entity<Category>()
            .HasData(new Category { Id = 1, Name = "仕事" });
    }
}
