using Items;
using Singleton;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Always-on-top overlay for the single item currently being dragged out of an InventorySlot.
/// Owns one reusable Item "cursor" instance (toggled active/inactive, never destroyed) so dragging
/// never has to spawn/destroy a GameObject per grab.
/// </summary>
public class DragLayer : SingletonBase<DragLayer>
{
    private RectTransform _rectTransform;
    private Item _cursorItem;

    public void BeginDrag(ItemData data, int amount, InventorySlot origin, Vector2 screenPosition)
    {
        EnsureCreated();

        _cursorItem.gameObject.SetActive(true);
        _cursorItem.itemData = data;
        _cursorItem.carriedCount = amount;
        _cursorItem.originSlot = origin;
        _cursorItem.created = true;
        _cursorItem.Initalize();
        _cursorItem.WarpToScreenPoint(screenPosition);
    }

    public void UpdateDrag(Vector2 screenPosition)
    {
        if (_cursorItem != null && _cursorItem.gameObject.activeSelf)
        {
            _cursorItem.WarpToScreenPoint(screenPosition);
        }
    }

    public void EndDrag()
    {
        if (_cursorItem != null && _cursorItem.gameObject.activeSelf)
        {
            _cursorItem.CheckCollision();
        }
    }

    private void EnsureCreated()
    {
        if (_rectTransform != null) return;

        GameObject canvasGo = GameObject.Find("HUD Canvas");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;

        GameObject layerGo = new GameObject("DragLayer", typeof(RectTransform), typeof(Canvas));
        layerGo.transform.SetParent(canvasGo != null ? canvasGo.transform : null, false);

        _rectTransform = layerGo.GetComponent<RectTransform>();
        _rectTransform.anchorMin = Vector2.zero;
        _rectTransform.anchorMax = Vector2.one;
        _rectTransform.offsetMin = Vector2.zero;
        _rectTransform.offsetMax = Vector2.zero;

        Canvas overlayCanvas = layerGo.GetComponent<Canvas>();
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = 1000;

        GameObject cursorGo = new GameObject("DragCursorItem", typeof(RectTransform), typeof(Image), typeof(Item));
        cursorGo.transform.SetParent(_rectTransform, false);

        Image cursorImage = cursorGo.GetComponent<Image>();
        cursorImage.raycastTarget = false;

        _cursorItem = cursorGo.GetComponent<Item>();
        cursorGo.SetActive(false);
    }
}
