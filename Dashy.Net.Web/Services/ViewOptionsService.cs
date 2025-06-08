namespace Dashy.Net.Web.Services;


public class ViewOptionsService
{
    public bool IsEditMode { get; set; } = false;

    public event Action? OnEditModeChanged;

    public void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
        OnEditModeChanged?.Invoke();
    }

}
