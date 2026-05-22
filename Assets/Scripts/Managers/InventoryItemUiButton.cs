using Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiItemButton : MonoBehaviour
{
    public BuildingData buildingData;
    public TMP_Text priceText;
    public PlacementManager placementManager;

    public TMP_Text countText;

    private Vector3 originalScale;
    private Color originalColor = Color.white;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        Image img = GetComponent<Image>();
        if (img != null) originalColor = img.color;
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChange += RefreshUI;
        }
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.onCurrencyChange += RefreshUI;
        }
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.onZoneUnlock += RefreshUI;
        }
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChange -= RefreshUI;
        }
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.onCurrencyChange -= RefreshUI;
        }
        if (ZoneManager.Instance != null)
        {
            ZoneManager.Instance.onZoneUnlock -= RefreshUI;
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (buildingData != null)
        {
            bool isUnlocked = buildingData.IsUnlocked();

            if (priceText != null)
            {
                if (isUnlocked)
                {
                    var adjustedPrice = buildingData.cost * CurrencyManager.Instance.exchangeRate;
                    priceText.text = "$" + adjustedPrice;
                }
                else
                {
                    string reqs = buildingData.GetUnlockRequirementsText();
                    string firstReq = reqs.Contains("\n") ? reqs.Split('\n')[0] : reqs;
                    priceText.text = "LOCKED\n(" + firstReq + ")";
                }
            }

            if (countText != null)
            {
                if (isUnlocked)
                {
                    InventoryManager.InventoryItem item = InventoryManager.Instance.items.Find(i => i.data == buildingData);
                    countText.text = item != null ? item.count.ToString() : "0";
                }
                else
                {
                    countText.text = "";
                }
            }

            UpdateAffordabilityColor();
        }
    }

    private void UpdateAffordabilityColor()
    {
        Image img = GetComponent<Image>();
        if (img == null || buildingData == null || CurrencyManager.Instance == null) return;

        if (!buildingData.IsUnlocked())
        {
            img.color = new Color(0.25f, 0.25f, 0.25f, 0.6f);
            return;
        }

        Color targetColor = GetTargetColor();
        if (feedbackCoroutine == null)
        {
            img.color = targetColor;
        }
    }

    private Color GetTargetColor()
    {
        if (buildingData == null || CurrencyManager.Instance == null) return originalColor;
        float cost = buildingData.cost * CurrencyManager.Instance.exchangeRate;
        bool canAfford = CurrencyManager.Instance.currentCurrencyValue >= cost;
        return canAfford ? originalColor : new Color(1f, 0.45f, 0.45f, 1f); // soft vibrant red tint for unaffordable
    }

    private void StopActiveCoroutine()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
        transform.localScale = originalScale;
        transform.localRotation = Quaternion.identity;
    }

    // This is now purely for the STORE
    public void PurchaseBuilding()
    {
        if (buildingData == null) return;

        if (!buildingData.IsUnlocked())
        {
            Debug.Log(buildingData.buildingName + " is locked!");
            StopActiveCoroutine();
            feedbackCoroutine = StartCoroutine(ShakeAndRedFlash());
            return;
        }

        float cost = buildingData.cost * CurrencyManager.Instance.exchangeRate;
        if (CurrencyManager.Instance.currentCurrencyValue >= cost)
        {
            CurrencyManager.Instance.RemoveCurrency(buildingData.cost);
            InventoryManager.Instance.AddBuilding(buildingData, 1);
            
            // Auto-assign to first empty hotbar slot if it's the first time buying
            bool alreadyInHotbar = false;
            int emptySlot = -1;
            for (int i = 0; i < HotbarManager.Instance.slotCount; i++)
            {
                if (HotbarManager.Instance.Slots[i] == buildingData) alreadyInHotbar = true;
                if (emptySlot == -1 && HotbarManager.Instance.Slots[i] == null) emptySlot = i;
            }

            if (!alreadyInHotbar && emptySlot != -1)
            {
                HotbarManager.Instance.AssignToSlot(emptySlot, buildingData);
            }

            RefreshUI();

            // Visual feedback for successful purchase
            StopActiveCoroutine();
            feedbackCoroutine = StartCoroutine(PunchScaleAndColor());
            SpawnFloatingText("+1", new Color(0.2f, 0.9f, 0.2f, 1f));
        }
        else
        {
            Debug.Log("Not enough currency to buy " + buildingData.buildingName);
            // Visual feedback for failed purchase (insufficient funds)
            StopActiveCoroutine();
            feedbackCoroutine = StartCoroutine(ShakeAndRedFlash());
        }
    }

    private void SpawnFloatingText(string text, Color color)
    {
        // Create a new GameObject for the floating text under this button
        GameObject floatGo = new GameObject("FloatingText", typeof(RectTransform), typeof(TextMeshProUGUI));
        floatGo.transform.SetParent(this.transform, false);
        
        TextMeshProUGUI tmp = floatGo.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = 28;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        
        // Copy font if available in countText or priceText
        if (countText != null) tmp.font = countText.font;
        else if (priceText != null) tmp.font = priceText.font;
        
        // Start floating coroutine
        StartCoroutine(AnimateFloatingText(floatGo.GetComponent<RectTransform>(), tmp));
    }

    private System.Collections.IEnumerator AnimateFloatingText(RectTransform rect, TextMeshProUGUI textComp)
    {
        float duration = 0.6f;
        float elapsed = 0f;
        Vector2 startPos = Vector2.zero; // Local center of the button
        Vector2 endPos = startPos + new Vector2(0f, 60f); // Float up 60 pixels
        
        rect.anchoredPosition = startPos;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Ease out quad for movement
            float tEase = t * (2f - t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, tEase);
            
            // Fade out
            Color c = textComp.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            textComp.color = c;
            
            // Scale pulse
            rect.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one * 0.8f, t);
            
            yield return null;
        }
        
        Destroy(rect.gameObject);
    }

    private System.Collections.IEnumerator PunchScaleAndColor()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        
        Image img = GetComponent<Image>();
        Color startColor = img != null ? img.color : originalColor;
        Color flashColor = new Color(0.7f, 1f, 0.7f, 1f); // soft mint green highlight
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Scale bounce
            float scaleFactor = 1f;
            if (t < 0.2f)
                scaleFactor = Mathf.Lerp(1f, 0.88f, t / 0.2f);
            else if (t < 0.5f)
                scaleFactor = Mathf.Lerp(0.88f, 1.12f, (t - 0.2f) / 0.3f);
            else
                scaleFactor = Mathf.Lerp(1.12f, 1f, (t - 0.5f) / 0.5f);
            
            transform.localScale = originalScale * scaleFactor;
            
            // Color flash interpolation
            if (img != null)
            {
                Color endColor = GetTargetColor();
                if (t < 0.3f)
                    img.color = Color.Lerp(startColor, flashColor, t / 0.3f);
                else
                    img.color = Color.Lerp(flashColor, endColor, (t - 0.3f) / 0.7f);
            }
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        if (img != null) img.color = GetTargetColor();
        feedbackCoroutine = null;
    }

    private System.Collections.IEnumerator ShakeAndRedFlash()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        
        Image img = GetComponent<Image>();
        Color startColor = img != null ? img.color : originalColor;
        Color failColor = new Color(1f, 0.3f, 0.3f, 1f); // vibrant red flash
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            // Rock back and forth on Z axis
            float shakeAngle = Mathf.Sin(t * Mathf.PI * 6f) * 6f * (1f - t); 
            transform.localRotation = Quaternion.Euler(0f, 0f, shakeAngle);
            
            // Color flash
            if (img != null)
            {
                Color endColor = GetTargetColor();
                if (t < 0.2f)
                    img.color = Color.Lerp(startColor, failColor, t / 0.2f);
                else
                    img.color = Color.Lerp(failColor, endColor, (t - 0.2f) / 0.8f);
            }
            
            yield return null;
        }
        
        transform.localRotation = Quaternion.identity;
        if (img != null) img.color = GetTargetColor();
        feedbackCoroutine = null;
    }
}