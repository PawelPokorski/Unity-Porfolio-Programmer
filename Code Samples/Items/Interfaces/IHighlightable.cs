using UnityEngine.Events;

namespace Items
{
    public interface IHighlightable
    {
        bool IsHighlightable();
        UnityEvent<bool> OnObjectHighlight { get; set; }
    }
}