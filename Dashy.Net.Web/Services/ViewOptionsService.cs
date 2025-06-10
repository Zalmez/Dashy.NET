namespace Dashy.Net.Web.Services;

public enum ItemSize { Small, Medium, Large }
public enum LayoutOrientation { Horizontal, Vertical, Auto }
public class ViewOptionsService
{
    public bool IsEditMode { get; private set; } = false;
    public event Action? OnEditModeChanged;
    public event Action? OnSearchTermChanged;
    public ItemSize CurrentItemSize { get; private set; } = ItemSize.Medium;
    public event Action? OnItemSizeChanged;
    public string SearchTerm { get; private set; } = string.Empty;

    public LayoutOrientation CurrentLayout { get; private set; } = LayoutOrientation.Auto;
    public event Action? OnLayoutChanged;

    public void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
        OnEditModeChanged?.Invoke();
    }

    public void SetSearchTerm(string term)
    {
        SearchTerm = term;
        OnSearchTermChanged?.Invoke();
    }

    public void SetItemSize(ItemSize size)
    {
        if (CurrentItemSize == size) return;
        CurrentItemSize = size;
        OnItemSizeChanged?.Invoke();
    }

    public void SetLayout(LayoutOrientation layout)
    {
        if (CurrentLayout == layout) return;
        CurrentLayout = layout;
        OnLayoutChanged?.Invoke();
    }

}