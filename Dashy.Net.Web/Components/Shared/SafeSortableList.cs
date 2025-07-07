using BlazorSortable;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Dashy.Net.Web.Components.Shared
{
    public class SafeSortableList<TItem> : SortableList<TItem>
    {
        // Removed Dispose method as it is not defined in the base class
    }
}