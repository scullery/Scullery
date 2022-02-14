namespace Scullery;

public class JobCall
{
    public string Type { get; set; } = null!;
    public string Method { get; set; } = null!;
    public string Returns { get; set; } = null!;
    public bool IsStatic { get; set; }
    public object[] Arguments { get; set; } = null!;
}
