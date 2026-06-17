using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiZoneButton : MonoBehaviour
{
    public enum Direction { North, South, East, West }
    public Direction direction;

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
        if (CurrencyManager.HasInstance)
        {
            CurrencyManager.Instance.onCurrencyChange -= RefreshUI;
        }
        if (ZoneManager.HasInstance)
        {
            ZoneManager.Instance.onZoneUnlock -= RefreshUI;
        }
    }

    private void Start()
    {
        // Add listener dynamically
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnButtonClicked);
        }
        
        RefreshUI();
    }

    public void RefreshUI()
    {
        UpdateAffordabilityColor();
        UpdateText();
    }

    private void UpdateText()
    {
        var textComp = GetComponentInChildren<TMP_Text>();
        if (textComp == null || ZoneManager.Instance == null) return;

        string dirStr = "EXPAND " + direction.ToString().ToUpper();
        float cost = ZoneManager.Instance.GetUnlockCost();
        textComp.text = dirStr + "\n$" + cost.ToString("F0");
    }

    private void UpdateAffordabilityColor()
    {
        Image img = GetComponent<Image>();
        if (img == null || CurrencyManager.Instance == null || ZoneManager.Instance == null) return;

        Vector2Int targetZone = GetTargetZone();
        if (ZoneManager.Instance.IsZoneUnlocked(targetZone))
        {
            // Already unlocked: semi-transparent faded look
            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
            return;
        }

        Color targetColor = GetTargetColor();
        if (feedbackCoroutine == null)
        {
            img.color = targetColor;
        }
    }

    private Vector2Int GetTargetZone()
    {
        if (ZoneManager.Instance == null) return Vector2Int.zero;
        Vector2Int current = ZoneManager.Instance.GetCurrentZone();
        switch (direction)
        {
            case Direction.North: return current + Vector2Int.up;
            case Direction.South: return current + Vector2Int.down;
            case Direction.East: return current + Vector2Int.right;
            case Direction.West: return current + Vector2Int.left;
        }
        return current;
    }

    private Color GetTargetColor()
    {
        if (ZoneManager.Instance == null || CurrencyManager.Instance == null) return originalColor;
        
        Vector2Int target = GetTargetZone();
        if (ZoneManager.Instance.IsZoneUnlocked(target))
        {
            return new Color(originalColor.r, originalColor.g, originalColor.b, 0.4f);
        }

        float cost = ZoneManager.Instance.GetUnlockCost();
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

    private void OnButtonClicked()
    {
        if (ZoneManager.Instance == null) return;

        Vector2Int targetZone = GetTargetZone();
        if (ZoneManager.Instance.IsZoneUnlocked(targetZone))
        {
            // Already unlocked: do absolutely nothing so we do not hijack the camera panning from the player!
            return;
        }

        bool success = false;
        switch (direction)
        {
            case Direction.North: success = ZoneManager.Instance.UnlockNorth(); break;
            case Direction.South: success = ZoneManager.Instance.UnlockSouth(); break;
            case Direction.East: success = ZoneManager.Instance.UnlockEast(); break;
            case Direction.West: success = ZoneManager.Instance.UnlockWest(); break;
        }

        if (success)
        {
            RefreshUI();
            StopActiveCoroutine();
            feedbackCoroutine = StartCoroutine(PunchScaleAndColor());
            SpawnFloatingText("UNLOCKED!", new Color(0.2f, 0.9f, 0.2f, 1f));
        }
        else
        {
            StopActiveCoroutine();
            feedbackCoroutine = StartCoroutine(ShakeAndRedFlash());
        }
    }

    private void SpawnFloatingText(string text, Color color)
    {
        GameObject floatGo = new GameObject("FloatingZoneText", typeof(RectTransform), typeof(TextMeshProUGUI));
        floatGo.transform.SetParent(this.transform, false);
        
        TextMeshProUGUI tmp = floatGo.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.color = color;
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        
        var existingText = GetComponentInChildren<TMP_Text>();
        if (existingText != null) tmp.font = existingText.font;
        
        StartCoroutine(AnimateFloatingText(floatGo.GetComponent<RectTransform>(), tmp));
    }

    private System.Collections.IEnumerator AnimateFloatingText(RectTransform rect, TextMeshProUGUI textComp)
    {
        float duration = 0.75f;
        float elapsed = 0f;
        Vector2 startPos = Vector2.zero;
        Vector2 endPos = startPos + new Vector2(0f, 65f); // Float up 65 units
        
        rect.anchoredPosition = startPos;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            float tEase = t * (2f - t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, tEase);
            
            Color c = textComp.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            textComp.color = c;
            
            rect.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one * 0.8f, t);
            
            yield return null;
        }
        
        Destroy(rect.gameObject);
    }

    private System.Collections.IEnumerator PunchScaleAndColor()
    {
        float duration = 0.35f;
        float elapsed = 0f;
        
        Image img = GetComponent<Image>();
        Color startColor = img != null ? img.color : originalColor;
        Color flashColor = new Color(0.7f, 1f, 0.7f, 1f); // soft mint green highlight
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            float scaleFactor = 1f;
            if (t < 0.2f)
                scaleFactor = Mathf.Lerp(1f, 0.86f, t / 0.2f);
            else if (t < 0.5f)
                scaleFactor = Mathf.Lerp(0.86f, 1.15f, (t - 0.2f) / 0.3f);
            else
                scaleFactor = Mathf.Lerp(1.15f, 1f, (t - 0.5f) / 0.5f);
            
            transform.localScale = originalScale * scaleFactor;
            
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
        Color failColor = new Color(1f, 0.3f, 0.3f, 1f);
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            float shakeAngle = Mathf.Sin(t * Mathf.PI * 6f) * 6f * (1f - t); 
            transform.localRotation = Quaternion.Euler(0f, 0f, shakeAngle);
            
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
