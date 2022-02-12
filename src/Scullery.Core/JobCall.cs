namespace Scullery;

public class JobCall
{
    public string Type { get; set; }
    public string Method { get; set; }
    public string Returns { get; set; }
    public bool IsStatic { get; set; }
    public object[] Arguments { get; set; }
}
