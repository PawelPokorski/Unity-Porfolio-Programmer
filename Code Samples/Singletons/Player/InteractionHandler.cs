using Items;
using Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Singletons.Player
{
    public class InteractionHandler : Singleton<InteractionHandler>
    {
        [Header("References")]
        [SerializeField] private Transform _player;
        private Transform _camera;

        [Header("Object Detection Settings")]
        [SerializeField] private Vector3 _boxSize;
        [SerializeField] private Vector2 _boxOffset;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private LayerMask _obstacleLayer;
        [SerializeField, Range(0f, 90f)] private float _maxFocusAngle = 45f;
        private Collider _collider;
        private readonly RaycastHit[] _hitsBuffer = new RaycastHit[3];
        private IInteractable _lastInteractable;
        private IInteractable _currentInteractable;

        [Header("Focus Settings")]
        [SerializeField] private float _focusRadius = 10f;
        [SerializeField] private float _focusDuration = 3f;
        [SerializeField] private Volume _focusVolume;
        private Coroutine _focusCoroutine;
        private bool _isFocusing = false;
        private readonly List<IHighlightable> _highlightedObjects = new();

        #region Unity Methods

        private void Start()
        {
            _collider = _player.GetComponent<Collider>();
            _camera = Camera.main.transform;
        }

        private void Update()
        {
            UpdateInteractableBuffer();

            if (UserInput.Instance.InteractPressed)
            {
                InteractWithObject(InteractionType.Primary);
            }
            else if (UserInput.Instance.SecondInteractPressed)
            {
                InteractWithObject(InteractionType.Secondary);
            }

            if (UserInput.Instance.FocusPressed)
            {
                if (!_isFocusing)
                {
                    if (_focusCoroutine != null)
                        StopCoroutine(_focusCoroutine);

                    _focusCoroutine = StartCoroutine(ActivateFocusMode());
                }
                else
                {
                    if (_focusCoroutine != null)
                        StopCoroutine(_focusCoroutine);
                    _focusCoroutine = StartCoroutine(DeactivateFocusMode());
                }
            }
        }

        #endregion

        /// <summary>
        /// Updates the currently focused interactable and fires availability events if it changed.
        /// </summary>
        private void UpdateInteractableBuffer()
        {
            Vector3 forward2D = new Vector3(_player.forward.x, 0f, _player.forward.z).normalized;
            Vector3 boxCenter = _collider.bounds.center + Vector3.up * _boxOffset.y + forward2D * _boxOffset.x;
            Quaternion boxRotation = Quaternion.LookRotation(forward2D, Vector3.up);

            int hitCount = Physics.BoxCastNonAlloc(
                boxCenter,
                _boxSize / 2f,
                forward2D,
                _hitsBuffer,
                boxRotation,
                0f,
                _interactionLayer
            );

            _currentInteractable = FindBestInteractable(hitCount);

            // fire events only when interactable changes
            if (_currentInteractable != _lastInteractable)
            {
                _lastInteractable?.OnObjectUnfocus.Invoke();
                _currentInteractable?.OnObjectFocus.Invoke();
            }

            _lastInteractable = _currentInteractable;
        }

        /// <summary>
        /// Finds the best interactable (smallest angle, visible, within focus cone).
        /// </summary>
        private IInteractable FindBestInteractable(int hitCount)
        {
            IInteractable best = null;
            float bestAngle = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitsBuffer[i];
                if (hit.collider == null || !hit.collider.TryGetComponent<IInteractable>(out var interactable))
                    continue;

                var center = hit.collider.bounds.center;

                // check angle first (cheap)
                float angle = CalculateAngle(_camera, center);
                if (angle > _maxFocusAngle)
                    continue;

                // then check visibility (expensive raycast)
                if (!IsVisible(interactable, center))
                    continue;

                if (angle < bestAngle)
                {
                    best = interactable;
                    bestAngle = angle;
                }
            }

            return best;
        }

        /// <summary>
        /// Returns true if the interactable is visible (not blocked by obstacles).
        /// </summary>
        private bool IsVisible(IInteractable interactable, Vector3 targetCenter)
        {
            Vector3 direction = (targetCenter - _camera.position).normalized;
            float distance = Vector3.Distance(_camera.position, targetCenter);

            if (Physics.Raycast(_camera.position, direction, out RaycastHit hit, distance, _obstacleLayer))
            {
                var hitInteractable = hit.collider.GetComponentInParent<IInteractable>();
                bool isSameInteractable = hitInteractable == interactable;
                bool isInteractionLayer = (_interactionLayer.value & (1 << hit.collider.gameObject.layer)) != 0;

                if (!isSameInteractable || !isInteractionLayer)
                    return false;
            }

            return true;
        }

        private void InteractWithObject(InteractionType type)
        {
            if (_currentInteractable.IsInteractable())
            {
                _currentInteractable?.OnObjectInteract?.Invoke(type);
                _lastInteractable = null;
            }
        }

        private static float CalculateAngle(Transform origin, Vector3 target)
        {
            Vector3 direction = (target - origin.position).normalized;
            return Vector3.Angle(origin.forward, direction);
        }

        /// <summary>
        /// Activates the focus mode, highlighting nearby objects and applying post-processing effects.
        /// </summary>
        private IEnumerator ActivateFocusMode()
        {
            _isFocusing = true;

            float elapsed = 0f;
            float fadeInDuration = 0.3f;

            StartCoroutine(FadeVolumeWeight(0f, 1f, fadeInDuration));

            _highlightedObjects.Clear();

            Collider[] colliders = new Collider[25];

            while (elapsed < _focusDuration && _isFocusing)
            {
                int count = Physics.OverlapSphereNonAlloc(_player.position, _focusRadius, colliders);

                HashSet<IHighlightable> current = new();

                for (int i = 0; i < count; i++)
                {
                    var col = colliders[i];
                    if (col == null) continue;

                    if (col.TryGetComponent<IHighlightable>(out var highlightable))
                    {
                        current.Add(highlightable);

                        if (!_highlightedObjects.Contains(highlightable))
                        {
                            highlightable.OnObjectHighlight?.Invoke(true);
                            _highlightedObjects.Add(highlightable);
                        }
                    }
                }

                for (int i = _highlightedObjects.Count - 1; i >= 0; i--)
                {
                    var h = _highlightedObjects[i];
                    if (h == null || !current.Contains(h))
                    {
                        h?.OnObjectHighlight?.Invoke(false);
                        _highlightedObjects.RemoveAt(i);
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            yield return StartCoroutine(DeactivateFocusMode());
        }

        private IEnumerator DeactivateFocusMode()
        {
            foreach (var obj in _highlightedObjects)
            {
                if (obj.IsHighlightable())
                {
                    obj?.OnObjectHighlight.Invoke(false);
                }
            }

            _highlightedObjects.Clear();

            yield return StartCoroutine(FadeVolumeWeight(1f, 0f, 0.3f));

            _isFocusing = false;
        }

        /// <summary>
        /// Smoothly interpolates the weight of the post-processing volume.
        /// </summary>
        private IEnumerator FadeVolumeWeight(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                if (_focusVolume != null)
                    _focusVolume.weight = Mathf.Lerp(from, to, t / duration);

                t += Time.deltaTime;
                yield return null;
            }

            if (_focusVolume != null)
                _focusVolume.weight = to;
        }
    }
}