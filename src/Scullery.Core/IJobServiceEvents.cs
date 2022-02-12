namespace Scullery;

public interface IJobServiceEvents
{
    Task OnStartedAsync();
    Task OnStoppedAsync();
    Task OnFailedAsync(Exception ex);
}
