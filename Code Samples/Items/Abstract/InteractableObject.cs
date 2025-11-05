using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Items
{
    [RequireComponent(typeof(Renderer), typeof(Collider))]
    public abstract class InteractableObject : MonoBehaviour, IInteractable, IHighlightable
    {
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;

        // IInteractable implementation
        public abstract bool IsInteractable();

        public abstract UnityEvent OnObjectFocus { get; set; }
        public abstract UnityEvent OnObjectUnfocus { get; set; }
        public abstract UnityEvent<InteractionType> OnObjectInteract { get; set; }

        // IHighlightable implementation
        private float _outlineThickness;
        private float _maxOutlineThickness;
        protected Color OutlineColor { get; set; }

        public abstract bool IsHighlightable();

        public abstract UnityEvent<bool> OnObjectHighlight { get; set; }

        private static readonly int EnableOutlineID = Shader.PropertyToID("_EnableOutline");
        private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");

        protected virtual void OnEnable()
        {
            OnObjectHighlight.AddListener((state) => SetHighlight(state));
        }

        protected virtual void OnDisable()
        {
            OnObjectFocus.RemoveAllListeners();
            OnObjectUnfocus.RemoveAllListeners();
            OnObjectInteract.RemoveAllListeners();
            OnObjectHighlight.RemoveAllListeners();
        }

        protected virtual void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();

            _renderer.SetPropertyBlock(_mpb);
            _maxOutlineThickness = _mpb.GetFloat(OutlineThicknessID);
        }

        private void SetHighlight(bool highlight)
        {
            StartCoroutine(FadeFocusIn(0.3f, highlight));
        }

        private IEnumerator FadeFocusIn(float duration, bool enable)
        {
            _renderer.GetPropertyBlock(_mpb);

            if (enable)
            {
                _mpb.SetFloat(EnableOutlineID, 1f);
                _mpb.SetColor(OutlineColorID, OutlineColor);
                _renderer.SetPropertyBlock(_mpb);
            }

            float start = enable ? 0f : 0.05f;
            float end = enable ? 0.05f : 0f;

            float t = 0f;
            while (t < duration)
            {
                float factor = t / duration;
                _outlineThickness = Mathf.Lerp(start, end, factor);

                _mpb.SetFloat(OutlineThicknessID, _outlineThickness);
                _renderer.SetPropertyBlock(_mpb);

                t += Time.deltaTime;
                yield return null;
            }

            _mpb.SetFloat(OutlineThicknessID, end);
            _renderer.SetPropertyBlock(_mpb);

            if (!enable)
            {
                _mpb.SetFloat(EnableOutlineID, 0f);
                _renderer.SetPropertyBlock(_mpb);
            }
        }

    }
}