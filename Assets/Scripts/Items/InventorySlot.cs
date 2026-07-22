using System;
using Code.Scripts.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Items
{
    public class InventorySlot : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IDragHandler, IEndDragHandler
    {
        public ItemData _itemData;
        public int itemCount;
        public Image _image;
        public bool slotFilled;
        private RectTransform _rectTransform;
        [SerializeField] private TMP_Text textPrefab;
        [SerializeField] private Vector2 textSpawnOffset = new Vector2(0, 5);
        [SerializeField] private float textSpacing = 20f;
        private TMP_Text _nameText;
        private TMP_Text _countText;

        public event Action<InventorySlot> OnSlotChanged;
        public event Action<InventorySlot> OnSlotHovered;
        public event Action<InventorySlot> OnSlotUnhovered;

        private void Start()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            var textOffset = textSpawnOffset;

            _nameText = Instantiate(textPrefab, _rectTransform);
            _nameText.transform.SetParent(_rectTransform);
            _nameText.rectTransform.anchoredPosition += textOffset;
            _countText = Instantiate(textPrefab, _rectTransform);
            _countText.transform.SetParent(_rectTransform);
            _countText.rectTransform.anchoredPosition += textOffset + new Vector2(0, textSpacing);
            EventManager.Instance.Subscribe(this, (CollisionItemExchangeEvent e) => CheckCollision(e.rectangle, e.item));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSlotHovered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnSlotUnhovered?.Invoke(this);
        }

        public Rectangle2D GetBoundingBox()
        {
            float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
            return TwoDCollision.CreateFromRotated(_rectTransform.position.x, _rectTransform.position.y, _rectTransform.sizeDelta.x, _rectTransform.sizeDelta.y, angleRadians);
        }

        /// <summary>Adds `amount` of `data` to this slot if it's empty or already holds the same specificItemType. Returns whether the add happened.</summary>
        public bool TryAdd(ItemData data, int amount)
        {
            if (data == null || amount <= 0) return false;
            if (slotFilled && _itemData.specificItemType != data.specificItemType) return false;

            if (!slotFilled)
            {
                slotFilled = true;
                _itemData = data;
                _image.sprite = data.sprite;
                _nameText.text = data.itemName;
            }

            itemCount += amount;
            _countText.text = itemCount.ToString();
            OnSlotChanged?.Invoke(this);
            return true;
        }

        /// <summary>Removes up to `amount` from this slot, clearing it if it hits 0. Returns how much was actually removed.</summary>
        public int TryRemove(int amount)
        {
            if (!slotFilled || amount <= 0) return 0;

            int removed = Mathf.Min(amount, itemCount);
            itemCount -= removed;
            _countText.text = itemCount.ToString();

            if (itemCount <= 0)
            {
                Clear();
            }
            else
            {
                OnSlotChanged?.Invoke(this);
            }

            return removed;
        }

        public void Clear()
        {
            slotFilled = false;
            _itemData = null;
            itemCount = 0;
            _image.sprite = null;
            _nameText.text = "";
            _countText.text = "";
            OnSlotChanged?.Invoke(this);
        }

        public void CheckCollision(Rectangle2D itemBox, Item item)
        {
            Rectangle2D boundingBox = GetBoundingBox();

            if (Rectangle2D.CheckCollision(boundingBox, itemBox))
            {
                if (TryAdd(item.itemData, item.carriedCount))
                {
                    item.MarkClaimed();
                }
            }
        }

        /// <summary>Grabs this slot's contents instantly on press - left click takes 1, right click
        /// takes the whole stack - and hands it to the always-on-top DragLayer cursor to follow the
        /// mouse from this exact frame, no separate drag-start step required.</summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!slotFilled) return;

            int requested = eventData.button == PointerEventData.InputButton.Right ? itemCount : 1;
            ItemData grabbedData = _itemData;
            int removed = TryRemove(requested);
            if (removed <= 0) return;

            DragLayer.Instance.BeginDrag(grabbedData, removed, this, eventData.position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            DragLayer.Instance.UpdateDrag(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            DragLayer.Instance.EndDrag();
        }
    }


}
