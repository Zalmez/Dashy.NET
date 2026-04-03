namespace dashy3.Web.Services;

/// <summary>
/// Service for managing search state across components.
/// </summary>
public interface ISearchService
{
    string SearchQuery { get; }
    event Action? SearchChanged;
    void SetSearchQuery(string query);
    bool Matches(string text);
}

public class SearchService : ISearchService
{
    private string _searchQuery = "";

    public string SearchQuery => _searchQuery;

    public event Action? SearchChanged;

    public void SetSearchQuery(string query)
    {
        if (_searchQuery != query)
        {
            _searchQuery = query ?? "";
            SearchChanged?.Invoke();
        }
    }

    /// <summary>
    /// Check if the given text matches the current search query.
    /// </summary>
    public bool Matches(string text)
    {
        if (string.IsNullOrWhiteSpace(_searchQuery))
            return true;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        return text.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase);
    }
}
