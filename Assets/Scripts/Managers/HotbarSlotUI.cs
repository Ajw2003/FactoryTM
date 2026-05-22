using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HotbarSlotUI : MonoBehaviour, IPointerClickHandler
{
    public Image iconImage;
    public TMP_Text countText;
    public Image highlightFrame;

    private int slotIndex;
    private BuildingData currentData;
    
    private Vector3 originalScale = Vector3.one;
    private Coroutine pulseCoroutine;
    private BuildingData lastData;
    private int lastCount = -1;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HotbarManager.Instance != null)
        {
            HotbarManager.Instance.SelectSlot(slotIndex);
        }
    }

    public void UpdateSlot(BuildingData data, bool isSelected)
    {
        // Calculate new count
        int newCount = 0;
        if (data != null)
        {
            var item = InventoryManager.Instance.items.Find(i => i.data == data);
            newCount = item != null ? item.count : 0;
        }

        // Detect if item is newly added or count increased
        bool newlyAdded = (data != null && lastData == null);
        bool countIncreased = (data != null && lastData == data && newCount > lastCount);

        currentData = data;
        if (highlightFrame != null) highlightFrame.enabled = isSelected;

        if (data != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.enabled = data.icon != null;
            }
            
            if (countText != null)
            {
                countText.text = newCount.ToString();
            }

            // Trigger visual feedback if newly added or count increased
            if (newlyAdded || countIncreased)
            {
                if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
                pulseCoroutine = StartCoroutine(PulseSlot(newlyAdded));
                
                // Spawn a floating +1 above the hotbar slot
                Color textCol = newlyAdded ? new Color(1f, 0.85f, 0.4f, 1f) : new Color(0.36f, 1f, 0.36f, 1f);
                SpawnFloatingText("+1", textCol);
            }
        }
        else
        {
            if (iconImage != null) iconImage.enabled = false;
            if (countText != null) countText.text = "";
        }

        // Cache for next comparison
        lastData = data;
        lastCount = newCount;
    }

    private System.Collections.IEnumerator PulseSlot(bool isNewItem)
    {
        float duration = isNewItem ? 0.35f : 0.25f;
        float elapsed = 0f;
        
        Image bgImg = GetComponent<Image>();
        Color originalBgColor = bgImg != null ? bgImg.color : Color.white;
        
        // Success flash color: bright gold/yellow for a brand new item, mint green for adding count
        Color flashColor = isNewItem ? new Color(1f, 0.85f, 0.4f, 1f) : new Color(0.6f, 1f, 0.6f, 1f);
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Nice scale curve
            float scaleFactor = 1f;
            if (isNewItem)
            {
                // Extra bounce for new item
                if (t < 0.25f)
                    scaleFactor = Mathf.Lerp(1f, 1.28f, t / 0.25f);
                else if (t < 0.6f)
                    scaleFactor = Mathf.Lerp(1.28f, 0.9f, (t - 0.25f) / 0.35f);
                else
                    scaleFactor = Mathf.Lerp(0.9f, 1f, (t - 0.6f) / 0.4f);
            }
            else
            {
                // Quick count increase pulse
                if (t < 0.3f)
                    scaleFactor = Mathf.Lerp(1f, 1.16f, t / 0.3f);
                else
                    scaleFactor = Mathf.Lerp(1.16f, 1f, (t - 0.3f) / 0.7f);
            }
            
            transform.localScale = originalScale * scaleFactor;
            
            // Background flash lerp
            if (bgImg != null)
            {
                if (t < 0.2f)
                    bgImg.color = Color.Lerp(originalBgColor, flashColor, t / 0.2f);
                else
                    bgImg.color = Color.Lerp(flashColor, originalBgColor, (t - 0.2f) / 0.8f);
            }
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        if (bgImg != null) bgImg.color = originalBgColor;
    }

    private void SpawnFloatingText(string text, Color color)
    {
        GameObject floatGo = new GameObject("FloatingSlotText", typeof(RectTransform), typeof(TextMeshProUGUI));
        floatGo.transform.SetParent(this.transform, false);
        
        TextMeshProUGUI tmp = floatGo.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        
        if (countText != null) tmp.font = countText.font;
        
        StartCoroutine(AnimateFloatingText(floatGo.GetComponent<RectTransform>(), tmp));
    }

    private System.Collections.IEnumerator AnimateFloatingText(RectTransform rect, TextMeshProUGUI textComp)
    {
        float duration = 0.55f;
        float elapsed = 0f;
        Vector2 startPos = new Vector2(0f, 15f); // Start slightly above slot center
        Vector2 endPos = startPos + new Vector2(0f, 45f); // Float up 45 pixels
        
        rect.anchoredPosition = startPos;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            float tEase = t * (2f - t); // Ease out
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, tEase);
            
            Color c = textComp.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            textComp.color = c;
            
            rect.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one * 0.8f, t);
            
            yield return null;
        }
        
        Destroy(rect.gameObject);
    }
}
