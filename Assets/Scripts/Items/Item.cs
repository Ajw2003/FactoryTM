using System;
using System.Runtime.CompilerServices;
using Code.Scripts.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Items
{
    public class Item : MonoBehaviour,IDragHandler, IBeginDragHandler, IEndDragHandler
    {

        public ItemData itemData;

        public bool created;
    
        protected Image _image;

        public string itemName;
        
        public string description;
        
        public specificItemType _itemType;
        
        protected RectTransform _rectTransform;

        protected Vector2 _startingTransform;
        
        protected Canvas parentCanvas;

        protected void Start()
        {
            if(created) return;
            Initalize();
        }

        public void Initalize()
        {
            _image = GetComponent<Image>();
            parentCanvas = gameObject.GetComponentInParent<Canvas>();
            _rectTransform = gameObject.GetComponent<RectTransform>();
            
            _image.sprite = itemData.sprite;
            itemName = itemData.itemName;
            description = itemData.itemDescription;
            _itemType = itemData.specificItemType;
            _startingTransform = _rectTransform.anchoredPosition;
        }

        public void ResetPosition()
        {
            _rectTransform.anchoredPosition = _startingTransform;
        }
        
        public Rectangle2D GetBoundingBox()
        {
            float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
            return TwoDCollision.CreateFromRotated(_rectTransform.position.x, _rectTransform.position.y, _rectTransform.sizeDelta.x, _rectTransform.sizeDelta.y, angleRadians);
        }

        public void CheckCollision()
        {
            Debug.Log("colliding");
            Rectangle2D boundingBox = GetBoundingBox();
            EventManager.Instance.Publish(new CollisionItemExchangeEvent{ rectangle =  boundingBox, item = this});
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 screenPoint = eventData.position;

            // Convert into the space of whatever this item is actually parented
            // to right now (Canvas in the no-layout-group case, the InventorySlot
            // when a HorizontalLayoutGroup put it there) instead of always assuming
            // the Canvas. anchoredPosition is relative to the immediate parent, so
            // using the wrong space here is what made dragging fly off/jitter when
            // a layout group was involved.
            RectTransform targetSpace = _rectTransform.parent as RectTransform;
            if (targetSpace == null) return;
    
            Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetSpace, screenPoint, cam, out Vector2 localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
            //move while dragging do transform logic here
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
           
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CheckCollision();
        }
    }
}