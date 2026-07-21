using System;
using Code.Scripts.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Items
{
    public class InventorySlot : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
    {
        Item _currentItem;
        public int itemCount;
        public Image _image;
        public bool slotFilled;
        private RectTransform _rectTransform;
        [SerializeField] private TMP_Text textPrefab;
        [SerializeField] private Vector2 textSpawnOffset = new Vector2(0, 5);
        [SerializeField] private float textSpacing = 20f;
        private TMP_Text _nameText;
        private TMP_Text _countText;

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
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            EventManager.Instance.Subscribe(this, (CollisionItemExchangeEvent e) => CheckCollision(e.rectangle, e.item));
            //if slot filled fire event with slot item listed
            // if slot not filled fire event saying slot is empty
        }
        
        
        public Rectangle2D GetBoundingBox()
        {
            float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
            return TwoDCollision.CreateFromRotated(_rectTransform.position.x, _rectTransform.position.y, _rectTransform.sizeDelta.x, _rectTransform.sizeDelta.y, angleRadians);
        }

        private void IncreaseCount()
        {
            itemCount++;
            _countText.text = itemCount.ToString();
        }
        
        
        public void CheckCollision(Rectangle2D itemBox, Item item)
        {

            Rectangle2D boundingBox = GetBoundingBox();
            
            if (Rectangle2D.CheckCollision(boundingBox, itemBox))
            {
                if (slotFilled)
                {
                    if (item.resourceType == _currentItem.resourceType) { IncreaseCount(); Destroy(item.gameObject);}
                    else item.ResetPosition();
                }
                else
                {
                    slotFilled = true;
                    _currentItem = item;
                    IncreaseCount();
                    _nameText.text = item.name;
                    _image.sprite = item.itemData.sprite;
                    Destroy(item.gameObject);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            EventManager.Instance.Unsubscribe<CollisionItemExchangeEvent>(this);
        }
    }
    
   
}
