namespace Tasks;

public class Task
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Assignee { get; set; }
    public bool IsComplete { get; set; }
}