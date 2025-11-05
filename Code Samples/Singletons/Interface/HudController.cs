using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace Singletons.Interface
{
    public class HudController : Singleton<HudController>
    {
        private VisualElement _root;
        private VisualElement _itemTooltip;

        private Vector3 _focusedItemPosition = Vector3.zero;

        private Camera _mainCamera;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _itemTooltip = _root.Q<VisualElement>("itemTooltip");
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            HideItemTooltip();
        }

        private void Update()
        {
            UpdateItemTooltip();
        }

        private void UpdateItemTooltip()
        {
            if (_focusedItemPosition == Vector3.zero)
                return;

            Vector3 screenPosition = _mainCamera.WorldToScreenPoint(_focusedItemPosition + Vector3.up * 0.1f);

            float uiY = _root.resolvedStyle.height - screenPosition.y;

            _itemTooltip.style.left = screenPosition.x;
            _itemTooltip.style.top = uiY;
        }

        public void ShowItemTooltip(ItemData itemData, Vector3 itemPosition, bool canPickup = true, bool canReplace = false)
        {
            var pickupAction = _itemTooltip.Q<VisualElement>("pickupAction");
            var replaceAction = _itemTooltip.Q<VisualElement>("replaceAction");
            var inventoryFullInfo = _itemTooltip.Q<VisualElement>("inventoryFullInfo");

            var itemNameLabel = _itemTooltip.Q<Label>("infoName");
            var itemTypeLabel = _itemTooltip.Q<Label>("infoType");

            itemNameLabel.text = itemData.name;
            itemTypeLabel.text = $"{itemData.GetItemRarity()} {itemData.GetItemType()}";

            itemTypeLabel.style.color = itemData.GetItemRarityColor();

            pickupAction.style.display = DisplayStyle.None;
            replaceAction.style.display = DisplayStyle.None;
            inventoryFullInfo.style.display = DisplayStyle.None;

            if (canPickup && !canReplace)
            {
                pickupAction.style.display = DisplayStyle.Flex;
            }
            else if (!canPickup && canReplace)
            {
                replaceAction.style.display = DisplayStyle.Flex;
            }
            else
            {
                inventoryFullInfo.style.display = DisplayStyle.Flex;
            }

            _focusedItemPosition = itemPosition;
            _itemTooltip.style.visibility = Visibility.Visible;
        }

        public void HideItemTooltip()
        {
            _focusedItemPosition = Vector3.zero;
            _itemTooltip.style.visibility = Visibility.Hidden;
        }
    }
}