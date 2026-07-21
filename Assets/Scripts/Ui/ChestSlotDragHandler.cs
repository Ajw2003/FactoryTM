using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Ui
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    /// <summary>Drags a resource out of the currently open Chest's storage, dropping it into the player's resource inventory panel to withdraw.</summary>
    public class ChestSlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ResourceType resourceType;
        public Sprite iconSprite;

        private GameObject dragVisual;
        private Canvas parentCanvas;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!(BuildingUiManager.Instance != null && BuildingUiManager.Instance.CurrentOpenBuilding is Chest chest) || chest.GetStoredCount(resourceType) <= 0)
            {
                eventData.pointerDrag = null;
                return;
            }

            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null) return;

            dragVisual = new GameObject("ChestDragVisual_" + resourceType.ToString(), typeof(RectTransform), typeof(Image));
            dragVisual.transform.SetParent(parentCanvas.transform, false);
            dragVisual.transform.SetAsLastSibling();

            RectTransform rt = dragVisual.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50f, 50f);

            Image img = dragVisual.GetComponent<Image>();
            img.sprite = iconSprite;
            img.color = new Color(1f, 1f, 1f, 0.75f);
            img.raycastTarget = false;

            UpdatePosition(eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragVisual != null)
            {
                UpdatePosition(eventData.position);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragVisual != null)
            {
                Destroy(dragVisual);
                dragVisual = null;
            }

            if (BuildingUiManager.Instance != null && BuildingUiManager.Instance.IsMouseOverPlayerInventoryPanel(eventData.position))
            {
                BuildingUiManager.Instance.HandleChestWithdraw(resourceType);
            }
        }

        private void UpdatePosition(Vector2 screenPoint)
        {
            if (parentCanvas == null || dragVisual == null) return;

            RectTransform canvasRt = parentCanvas.transform as RectTransform;
            if (canvasRt == null) return;

            Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, cam, out Vector2 localPoint))
            {
                dragVisual.GetComponent<RectTransform>().anchoredPosition = localPoint;
            }
        }
    }

}
