using System;
using Code.Scripts.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Items
{
    public class InventorySlot : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    { 
        Item _currentItem;
        private ItemData _itemData;
        private Item _tempItem;
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
            EventManager.Instance.Subscribe(this, (CollisionItemExchangeEvent e) => CheckCollision(e.rectangle, e.item));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //fix this currently causing any item placed bellow on the heirarchy to block the raycast that would allow this 
            
            //if slot filled fire event with slot item listed
            // if slot not filled fire event saying slot is empty
        }

        public void SpawnItem()
        {
            //instantiate new item if slot filled and decrement item count, if last item set count to 0 and slot filled to false
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

        private void DecreaseCount()
        {
            itemCount--;
            _countText.text = itemCount.ToString();
            if (itemCount <= 0)
            {
                slotFilled = false;
                _currentItem = null;
                _image.sprite = null;
                _nameText.text = "";
                _countText.text = "";
            }
        }
        
        
        public void CheckCollision(Rectangle2D itemBox, Item item)
        {
            Debug.Log(item.itemData.itemName);

            Rectangle2D boundingBox = GetBoundingBox();
            
            if (Rectangle2D.CheckCollision(boundingBox, itemBox))
            {
                if (slotFilled)
                {
                    if (item.resourceType == _currentItem.resourceType) { IncreaseCount(); Destroy(item.gameObject); }
                    else item.ResetPosition();
                }
                else
                {
                    slotFilled = true;
                    _currentItem = item;
                    IncreaseCount();
                    _itemData = item.itemData;
                    _nameText.text = item.itemName;
                    _image.sprite = item.itemData.sprite;
                    Destroy(item.gameObject);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //EventManager.Instance.Unsubscribe<CollisionItemExchangeEvent>(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (slotFilled)
            {
                if (itemCount > 0)
                {
                    DecreaseCount();
                    var temp = new GameObject ("DragVisual", typeof(Item), typeof(Image));
                    RectTransform rt = temp.GetComponent<RectTransform>();
                    if (this.GetComponentInParent<HorizontalLayoutGroup>())
                    {
                        rt.SetParent(_rectTransform, worldPositionStays: false);
                        rt.localPosition = Vector3.zero;
                    }
                    else
                    {
                        rt.SetParent(CanvasSingleton.Instance.transform, true);
                        rt.anchoredPosition = _rectTransform.anchoredPosition;
                    }
                    Item tempItem = temp.GetComponent<Item>();
                    tempItem.itemData = _itemData;
                    tempItem.created = true;
                    tempItem.Initalize();
                }
                else
                {
                    slotFilled = false;
                    _currentItem = null;
                    _image.sprite = null;
                    _nameText.text = "";
                    
                }
            }
        }
    }
    
   
}
