var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();


public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? color { get; set; }

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
    public Category? Category { get; set; }
}
