namespace Dashy.Net.Web.Services;

/// <summary>
/// Holds transient drag state for dashboard widget drag & drop operations.
/// Scoped per user session.
/// </summary>
public class DragContextService
{
    private int? _currentItemId;
    private int? _originalParentItemId;
    private int? _originalSectionId;

    public int? CurrentItemId => _currentItemId;
    public int? OriginalParentItemId => _originalParentItemId;
    public int? OriginalSectionId => _originalSectionId;

    public void Set(int itemId, int? parentItemId, int sectionId)
    {
        _currentItemId = itemId;
        _originalParentItemId = parentItemId;
        _originalSectionId = sectionId;
    }

    public void Clear()
    {
        _currentItemId = null;
        _originalParentItemId = null;
        _originalSectionId = null;
    }
}
