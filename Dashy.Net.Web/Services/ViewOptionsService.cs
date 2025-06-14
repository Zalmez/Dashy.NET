namespace Dashy.Net.Web.Services;

public enum ItemSize { Small, Medium, Large }
public enum LayoutOrientation { Vertical, Auto }

public class ViewOptionsService
{
    public event Action? OnChange;

    public bool IsEditMode { get; private set; } = false;
    public ItemSize CurrentItemSize { get; private set; } = ItemSize.Medium;
    public string SearchTerm { get; private set; } = string.Empty;
    public LayoutOrientation CurrentLayout { get; private set; } = LayoutOrientation.Auto;

    public void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
        NotifyStateChanged();
    }

    public void SetSearchTerm(string term)
    {
        if (SearchTerm == term) return;
        SearchTerm = term;
        NotifyStateChanged();
    }

    public void SetItemSize(ItemSize size)
    {
        if (CurrentItemSize == size) return;
        CurrentItemSize = size;
        NotifyStateChanged();
    }

    public void SetLayout(LayoutOrientation layout)
    {
        if (CurrentLayout == layout) return;
        CurrentLayout = layout;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}