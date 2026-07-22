using Code.Scripts.EventSystems;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Items
{
    public class Item : MonoBehaviour
    {

        public ItemData itemData;

        public bool created;

        /// <summary>How many units this dragged instance represents (1 for a left-click grab, the
        /// whole stack for a right-click grab). Given back to originSlot if the drop isn't claimed.</summary>
        public int carriedCount = 1;

        [System.NonSerialized] public InventorySlot originSlot;

        protected Image _image;

        public string itemName;

        public string description;

        public specificItemType _itemType;

        protected RectTransform _rectTransform;

        protected Vector2 _startingTransform;

        protected Canvas parentCanvas;

        private bool _claimed;

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

        /// <summary>Converts a screen point into this item's parent space and snaps it there. Used both
        /// for the instant-pickup warp on grab and every frame while the drag continues.</summary>
        public void WarpToScreenPoint(Vector2 screenPoint)
        {
            RectTransform targetSpace = _rectTransform.parent as RectTransform;
            if (targetSpace == null) return;

            Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetSpace, screenPoint, cam, out Vector2 localPoint))
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }

        /// <summary>Called by an InventorySlot that accepted this drop, instead of destroying the
        /// (reused) GameObject outright.</summary>
        public void MarkClaimed()
        {
            _claimed = true;
        }

        /// <summary>Resolves the drop: publishes a collision check to every InventorySlot, and if none
        /// of them claimed it, gives carriedCount back to originSlot. Either way, hides the cursor.</summary>
        public void CheckCollision()
        {
            _claimed = false;
            Rectangle2D boundingBox = GetBoundingBox();
            EventManager.Instance.Publish(new CollisionItemExchangeEvent{ rectangle =  boundingBox, item = this});

            if (!_claimed)
            {
                originSlot?.TryAdd(itemData, carriedCount);
            }

            originSlot = null;
            gameObject.SetActive(false);
        }
    }
}