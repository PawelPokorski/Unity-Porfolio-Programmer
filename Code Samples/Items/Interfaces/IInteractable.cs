using UnityEngine.Events;

namespace Items
{
    public interface IInteractable
    {
        bool IsInteractable();
        UnityEvent OnObjectFocus { get; set; }
        UnityEvent<InteractionType> OnObjectInteract { get; set; }
        UnityEvent OnObjectUnfocus { get; set; }
    }

    public enum InteractionType
    {
        Primary,
        Secondary
    }
}