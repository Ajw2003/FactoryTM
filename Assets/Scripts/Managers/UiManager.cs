using Singleton;
using TMPro;
using UnityEngine;

public class UiManager : SingletonBase<UiManager>
{
   public TMP_Text currentCurrency;
   
   public GameObject StorePanel;
   bool flip = false; // Start closed

   private float lastCurrencyValue = -1f;
   private Coroutine currencyLerpCoroutine;
   private Coroutine currencyPulseCoroutine;

   protected override void Awake()
   {
      base.Awake();
      
      // Ensure initial state
      if (StorePanel != null) StorePanel.SetActive(flip);
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.E))
      {
         flip = !flip;
         if (StorePanel != null) StorePanel.SetActive(flip);
      }
   }

   public void UpdateCurrency(float value)
   {
      if (lastCurrencyValue < 0f)
      {
         // Initial call (e.g. at Start)
         lastCurrencyValue = value;
         if (currentCurrency != null)
         {
            currentCurrency.text = value.ToString("F0");
         }
         return;
      }

      float delta = value - lastCurrencyValue;
      if (Mathf.Abs(delta) < 0.01f) return;

      if (currentCurrency != null)
      {
         if (delta < 0)
         {
            // Subtraction! Spawn beautiful red floating text (coral red)
            SpawnCurrencyFloatingText("-" + Mathf.Abs(delta).ToString("F0"), new Color(1f, 0.36f, 0.36f, 1f));
            
            // Pulse/shrink currency text
            if (currencyPulseCoroutine != null) StopCoroutine(currencyPulseCoroutine);
            currencyPulseCoroutine = StartCoroutine(PulseCurrencyText(false));
         }
         else
         {
            // Addition! Spawn beautiful green floating text (mint green)
            SpawnCurrencyFloatingText("+" + delta.ToString("F0"), new Color(0.36f, 1f, 0.36f, 1f));
            
            // Pulse/grow currency text
            if (currencyPulseCoroutine != null) StopCoroutine(currencyPulseCoroutine);
            currencyPulseCoroutine = StartCoroutine(PulseCurrencyText(true));
         }

         // Animate number tick counter
         if (currencyLerpCoroutine != null) StopCoroutine(currencyLerpCoroutine);
         currencyLerpCoroutine = StartCoroutine(AnimateCurrencyCounter(lastCurrencyValue, value));
      }

      lastCurrencyValue = value;
   }

   private System.Collections.IEnumerator PulseCurrencyText(bool isAddition)
   {
      Transform t = currentCurrency.transform;
      Vector3 originalScale = Vector3.one;
      float duration = 0.25f;
      float elapsed = 0f;
      
      while (elapsed < duration)
      {
         elapsed += Time.unscaledDeltaTime;
         float percent = elapsed / duration;
         
         float scaleFactor = 1f;
         if (isAddition)
         {
            if (percent < 0.3f)
               scaleFactor = Mathf.Lerp(1f, 1.25f, percent / 0.3f);
            else
               scaleFactor = Mathf.Lerp(1.25f, 1f, (percent - 0.3f) / 0.7f);
         }
         else
         {
            if (percent < 0.3f)
               scaleFactor = Mathf.Lerp(1f, 0.82f, percent / 0.3f);
            else
               scaleFactor = Mathf.Lerp(0.82f, 1f, (percent - 0.3f) / 0.7f);
         }
         
         t.localScale = originalScale * scaleFactor;
         yield return null;
      }
      
      t.localScale = originalScale;
   }

   private void SpawnCurrencyFloatingText(string text, Color color)
   {
      if (currentCurrency == null) return;
      
      GameObject go = new GameObject("FloatingCurrencyChange", typeof(RectTransform), typeof(TextMeshProUGUI));
      go.transform.SetParent(currentCurrency.transform.parent, false);
      
      RectTransform rt = go.GetComponent<RectTransform>();
      RectTransform currencyRt = currentCurrency.GetComponent<RectTransform>();
      
      rt.anchorMin = currencyRt.anchorMin;
      rt.anchorMax = currencyRt.anchorMax;
      rt.pivot = currencyRt.pivot;
      
      // Position to the right of the currency text with a nice spacing offset
      rt.anchoredPosition = currencyRt.anchoredPosition + new Vector2(65f, 0f);
      
      TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
      tmp.text = text;
      tmp.color = color;
      tmp.fontSize = currentCurrency.fontSize * 0.9f;
      tmp.font = currentCurrency.font;
      tmp.fontStyle = FontStyles.Bold;
      tmp.alignment = TextAlignmentOptions.Left;
      
      StartCoroutine(AnimateCurrencyFloatingText(rt, tmp));
   }

   private System.Collections.IEnumerator AnimateCurrencyFloatingText(RectTransform rect, TextMeshProUGUI textComp)
   {
      float duration = 0.8f;
      float elapsed = 0f;
      Vector2 startPos = rect.anchoredPosition;
      Vector2 endPos = startPos + new Vector2(0f, 45f); // float up 45 units
      
      while (elapsed < duration)
      {
         elapsed += Time.unscaledDeltaTime;
         float t = elapsed / duration;
         
         // Smooth deceleration
         float tEase = t * (2f - t);
         rect.anchoredPosition = Vector2.Lerp(startPos, endPos, tEase);
         
         Color c = textComp.color;
         c.a = Mathf.Lerp(1f, 0f, t);
         textComp.color = c;
         
         if (t < 0.2f)
            rect.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one * 1.2f, t / 0.2f);
         else
            rect.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one * 1.0f, (t - 0.2f) / 0.8f);
            
         yield return null;
      }
      
      Destroy(rect.gameObject);
   }

   private System.Collections.IEnumerator AnimateCurrencyCounter(float startVal, float endVal)
   {
      float duration = 0.4f;
      float elapsed = 0f;
      
      while (elapsed < duration)
      {
         elapsed += Time.unscaledDeltaTime;
         float t = elapsed / duration;
         
         float tEase = t * (2f - t);
         float currentVal = Mathf.Lerp(startVal, endVal, tEase);
         
         if (currentCurrency != null)
         {
            currentCurrency.text = currentVal.ToString("F0");
         }
         yield return null;
      }
      
      if (currentCurrency != null)
      {
         currentCurrency.text = endVal.ToString("F0");
      }
   }

}
