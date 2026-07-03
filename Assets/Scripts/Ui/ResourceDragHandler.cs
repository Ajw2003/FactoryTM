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
    
    public class ResourceDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ResourceType resourceType;
        public Sprite iconSprite;
        
        private GameObject dragVisual;
        private Canvas parentCanvas;
    
        public void OnBeginDrag(PointerEventData eventData)
        {
            // Only allow drag if player actually has at least 1 unit of this resource
            if (BuildingUiManager.Instance == null || BuildingUiManager.Instance.GetResourceCount(resourceType) <= 0)
            {
                eventData.pointerDrag = null; // Cancel drag sequence
                return;
            }
    
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null) return;
            
            // Create a temporary visual clone for the drag icon
            dragVisual = new GameObject("DragVisual_" + resourceType.ToString(), typeof(RectTransform), typeof(Image));
            dragVisual.transform.SetParent(parentCanvas.transform, false);
            dragVisual.transform.SetAsLastSibling(); // Force render on top of everything!
            
            RectTransform rt = dragVisual.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(50f, 50f);
            
            Image img = dragVisual.GetComponent<Image>();
            img.sprite = iconSprite;
            img.color = new Color(1f, 1f, 1f, 0.75f); // Semi-transparent
            img.raycastTarget = false; // Critical: so it does not block the drop Raycast
    
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
    
            if (BuildingUiManager.Instance != null && BuildingUiManager.Instance.IsPanelOpen)
            {
                // Check if dropped within the bounds of the IDT Intake Port
                if (BuildingUiManager.Instance.IsMouseOverIntakeZone(eventData.position))
                {
                    BuildingUiManager.Instance.HandleResourceDropped(resourceType);
                }
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


