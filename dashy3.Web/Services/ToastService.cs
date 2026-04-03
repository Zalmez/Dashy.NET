namespace dashy3.Web.Services;

public class ToastService : IToastService
{
    public event Action<string, string>? OnToast;

    public void ShowSuccess(string message) => OnToast?.Invoke(message, "Success");
    public void ShowError(string message) => OnToast?.Invoke(message, "Error");
    public void ShowInfo(string message) => OnToast?.Invoke(message, "Info");
}
