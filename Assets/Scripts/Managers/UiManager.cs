using System;
using System.Linq;
using Singleton;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiManager : SingletonBase<UiManager>
{
   public TMP_Text currentCurrency;
   
   public GameObject StorePanel;
   public GameObject StatsPanel;
   public GameObject GameOverPanel;
   private bool isGameOver = false;

   private float lastCurrencyValue = -1f;
   private Coroutine currencyLerpCoroutine;
   private Coroutine currencyPulseCoroutine;
   
   [SerializeField] private TMP_Text healthText;
   [SerializeField] private TMP_Text staminaText;
   [SerializeField] private GameObject heartObject;
   [SerializeField] private GameObject playerStatsUi;

   public GameObject[] Hearts;
   
   private int heartCount;
   private GameObject staminaContainer;

   protected override void Awake()
   {
      persistBetweenScenes = false;
      base.Awake();

      if (StorePanel == null) StorePanel = GameObject.Find("StorePanel");
      if (StatsPanel == null) StatsPanel = GameObject.Find("StatsPanel");
      if (GameOverPanel == null) GameOverPanel = GameObject.Find("GameOverPanel");

      if (healthText == null)
      {
         GameObject go = GameObject.Find("HealthText");
         if (go != null) healthText = go.GetComponent<TMP_Text>();
      }

      if (currentCurrency == null)
      {
         GameObject go = GameObject.Find("CurrencyText");
         if (go != null) currentCurrency = go.GetComponent<TMP_Text>();
      }

      SetupStaminaUI();
   }

   private void SetupStaminaUI()
   {
      GameObject staminaGo = GameObject.Find("StaminaText");
      if (staminaGo != null)
      {
         staminaText = staminaGo.GetComponent<TMP_Text>();
         staminaContainer = staminaGo;
         return;
      }

      // Create stamina UI dynamically if not found
      if (healthText != null)
      {
         staminaGo = Instantiate(healthText.gameObject, playerStatsUi.transform.parent);
         staminaGo.name = "StaminaText";
         staminaText = staminaGo.GetComponent<TMP_Text>();
         staminaContainer = staminaGo;
         
         RectTransform rt = staminaGo.GetComponent<RectTransform>();
         rt.anchoredPosition += new Vector2(0, -30f); // Position below health
         
         staminaText.color = new Color(1f, 0.8f, 0.2f); // Retro gold/yellow for stamina
         staminaText.text = "STAMINA: 100 / 100";
         
         staminaContainer.SetActive(false); // Hide until unlocked
      }
   }

   public void AdjustHeartContainers(int maxHp)
   {
      if (Hearts == null)
      {
         Hearts = new GameObject[0];
      }

      if (Hearts.Length == maxHp) return;

      int oldLength = Hearts.Length;
      if (maxHp > oldLength)
      {
         Array.Resize(ref Hearts, maxHp);
         for (int i = oldLength; i < maxHp; i++)
         {
            Hearts[i] = Instantiate(heartObject, playerStatsUi.transform);
            Hearts[i].transform.localScale = new Vector3(150, 150, 0);
         }
      }
      else if (maxHp < oldLength)
      {
         for (int i = maxHp; i < oldLength; i++)
         {
            if (Hearts[i] != null)
            {
               Destroy(Hearts[i]);
            }
         }
         Array.Resize(ref Hearts, maxHp);
      }
   }

   public void UpdateHp(int currentHealth, int maxHp)
   {
      AdjustHeartContainers(maxHp);

      for (int i = 0; i < maxHp; i++)
      {
         if (Hearts[i] != null)
         {
            Hearts[i].SetActive(i < currentHealth);
         }
      }
   }

   public void UpdateStamina(float current, float max)
   {
      if (staminaText != null)
      {
         staminaText.text = $"STAMINA: {Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
      }
   }

   public void ShowGameOver()
   {
      if (GameOverPanel != null)
      {
         isGameOver = true;
         GameOverPanel.SetActive(true);
         Time.timeScale = 0f;
      }
   }

   public void RestartGame()
   {
      isGameOver = false;
      Time.timeScale = 1f;
      SceneManager.LoadScene(SceneManager.GetActiveScene().name);
   }

   private void Start()
   {
      Time.timeScale = 1f;
      if (StorePanel != null) StorePanel.SetActive(false);
      
      if (StatsPanel == null)
      {
         GenerateStatsUI();
      }
      if (StatsPanel != null) StatsPanel.SetActive(false);

      if (GameOverPanel != null) GameOverPanel.SetActive(false);
      isGameOver = false;

      if (GameManager.Instance != null && GameManager.Instance.playerController != null)
      {
         UpdateHp(GameManager.Instance.playerController.Health, GameManager.Instance.playerController.maxHealth);
      }
   }

   private void Update()
   {
      if (isGameOver) return;

      // Show stamina UI if dodge is unlocked
      if (staminaContainer != null && PlayerController.Instance != null)
      {
         if (PlayerController.Instance.canDodgeRoll && !staminaContainer.activeSelf)
         {
            staminaContainer.SetActive(true);
         }
      }
   }

   public void OpenStore()
   {
      if (StorePanel != null)
      {
         StoreUiScript storeUi = StorePanel.GetComponent<StoreUiScript>();
         if (storeUi != null)
         {
            storeUi.OpenStore();
         }
         else
         {
            StorePanel.SetActive(true);
         }
      }
   }

   public void CloseStore()
   {
      if (StorePanel != null)
      {
         StoreUiScript storeUi = StorePanel.GetComponent<StoreUiScript>();
         if (storeUi != null)
         {
            storeUi.CloseStore();
         }
         else
         {
            StorePanel.SetActive(false);
         }
      }
   }

   public void OpenStats()
   {
      if (StatsPanel == null) GenerateStatsUI();

      if (StatsPanel != null)
      {
         StatsUiScript statsUi = StatsPanel.GetComponent<StatsUiScript>();
         if (statsUi != null)
         {
            statsUi.OpenStats();
         }
         else
         {
            StatsPanel.SetActive(true);
         }
      }
   }

   private void GenerateStatsUI()
   {
      GameObject canvas = GameObject.Find("Canvas");
      if (canvas == null) return;

      // 1. Create Panel
      StatsPanel = new GameObject("StatsPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image));
      StatsPanel.transform.SetParent(canvas.transform, false);

      RectTransform panelRt = StatsPanel.GetComponent<RectTransform>();
      panelRt.anchorMin = new Vector2(0.75f, 0f);
      panelRt.anchorMax = new Vector2(1f, 1f);
      panelRt.pivot = new Vector2(1f, 0.5f);
      panelRt.offsetMin = Vector2.zero;
      panelRt.offsetMax = Vector2.zero;

      UnityEngine.UI.Image bg = StatsPanel.GetComponent<UnityEngine.UI.Image>();
      bg.color = new Color(0.01f, 0.05f, 0.01f, 0.95f);

      // 2. Add Title
      GameObject title = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
      title.transform.SetParent(StatsPanel.transform, false);
      RectTransform titleRt = title.GetComponent<RectTransform>();
      titleRt.anchorMin = new Vector2(0.5f, 1f);
      titleRt.anchorMax = new Vector2(0.5f, 1f);
      titleRt.pivot = new Vector2(0.5f, 1f);
      titleRt.anchoredPosition = new Vector2(0, -30);
      titleRt.sizeDelta = new Vector2(400, 60);

      TextMeshProUGUI titleTxt = title.GetComponent<TextMeshProUGUI>();
      titleTxt.text = "STATISTICS";
      titleTxt.fontSize = 48;
      titleTxt.alignment = TextAlignmentOptions.Center;
      titleTxt.fontStyle = FontStyles.Bold;
      titleTxt.color = new Color(0.2f, 1f, 0.2f, 1f);

      // 3. Create Scroll View Content area (StatsUiScript will populate this)
      GameObject content = new GameObject("Content", typeof(RectTransform));
      content.transform.SetParent(StatsPanel.transform, false);
      RectTransform contentRt = content.GetComponent<RectTransform>();
      contentRt.anchorMin = new Vector2(0f, 0f);
      contentRt.anchorMax = new Vector2(1f, 1f);
      contentRt.pivot = new Vector2(0.5f, 0.5f);
      contentRt.offsetMin = new Vector2(20, 20);
      contentRt.offsetMax = new Vector2(-20, -100);

      // 4. Attach script
      StatsPanel.AddComponent<StatsUiScript>();

      // 5. Add Close Button
      GameObject closeBtn = new GameObject("CloseBtn", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
      closeBtn.transform.SetParent(StatsPanel.transform, false);
      RectTransform closeRt = closeBtn.GetComponent<RectTransform>();
      closeRt.anchorMin = new Vector2(1f, 1f);
      closeRt.anchorMax = new Vector2(1f, 1f);
      closeRt.pivot = new Vector2(1f, 1f);
      closeRt.anchoredPosition = new Vector2(-20, -20);
      closeRt.sizeDelta = new Vector2(50, 50);
      
      UnityEngine.UI.Image closeBg = closeBtn.GetComponent<UnityEngine.UI.Image>();
      closeBg.color = new Color(0.05f, 0.15f, 0.05f, 0.85f);

      UnityEngine.UI.Button btn = closeBtn.GetComponent<UnityEngine.UI.Button>();
      btn.onClick.AddListener(() => {
          CloseStats();
          if (PauseManager.IsPaused && PauseManager.Instance != null)
          {
              PauseManager.Instance.ShowPauseMenuOnly();
          }
      });

      GameObject closeTxtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
      closeTxtObj.transform.SetParent(closeBtn.transform, false);
      RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
      closeTxtRt.anchorMin = Vector2.zero;
      closeTxtRt.anchorMax = Vector2.one;
      closeTxtRt.offsetMin = Vector2.zero;
      closeTxtRt.offsetMax = Vector2.zero;

      TextMeshProUGUI closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
      closeTxt.text = "X";
      closeTxt.fontSize = 24;
      closeTxt.alignment = TextAlignmentOptions.Center;
      closeTxt.fontStyle = FontStyles.Bold;
      closeTxt.color = new Color(0.2f, 1f, 0.2f, 1f);
   }

   public void CloseStats()
   {
      if (StatsPanel != null)
      {
         StatsUiScript statsUi = StatsPanel.GetComponent<StatsUiScript>();
         if (statsUi != null)
         {
            statsUi.CloseStats();
         }
         else
         {
            StatsPanel.SetActive(false);
         }
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
            var dollar = "$";
            currentCurrency.text = dollar + value.ToString("F1");
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
            SpawnCurrencyFloatingText("-$" + Mathf.Abs(delta).ToString("F1"), new Color(1f, 0.36f, 0.36f, 1f));
            
            // Pulse/shrink currency text
            if (currencyPulseCoroutine != null) StopCoroutine(currencyPulseCoroutine);
            currencyPulseCoroutine = StartCoroutine(PulseCurrencyText(false));
         }
         else
         {
            // Addition! Spawn beautiful green floating text (mint green)
            SpawnCurrencyFloatingText("+$" + delta.ToString("F1"), new Color(0.36f, 1f, 0.36f, 1f));
            
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
            currentCurrency.text = "$" + currentVal.ToString("F1");
         }
         yield return null;
      }
      
      if (currentCurrency != null)
      {
         currentCurrency.text = "$" + endVal.ToString("F1");
      }
   }

   [Header("Alerts")]
   public AudioClip buildingDamageAlertSound;
   public AudioClip outOfAmmoSound;
   public AudioClip noReserveAmmoSound;

   public void ShowAmmoAlert(string message, bool isReserve = false)
   {
       AudioClip clipToPlay = isReserve ? noReserveAmmoSound : outOfAmmoSound;
       if (clipToPlay != null && Code.Scripts.Audio.AudioManager.HasInstance)
       {
           Code.Scripts.EventSystems.EventManager.Instance?.Publish(new AudioClipEvent { Clip = clipToPlay, Channel = AudioChannel.Sfx, Volume = 1f, Duration = 0f });
       }

       GameObject canvas = GameObject.Find("Canvas");
       if (canvas != null)
       {
           GameObject alert = new GameObject("AmmoAlert", typeof(RectTransform), typeof(TextMeshProUGUI));
           alert.transform.SetParent(canvas.transform, false);
           
           RectTransform rt = alert.GetComponent<RectTransform>();
           rt.anchorMin = new Vector2(0.5f, 0.45f);
           rt.anchorMax = new Vector2(0.5f, 0.45f);
           rt.pivot = new Vector2(0.5f, 0.5f);
           rt.anchoredPosition = Vector2.zero;
           
           TextMeshProUGUI tmp = alert.GetComponent<TextMeshProUGUI>();
           tmp.text = message;
           tmp.color = new Color(1f, 0.2f, 0.2f);
           tmp.fontSize = 30;
           tmp.fontStyle = FontStyles.Bold;
           tmp.alignment = TextAlignmentOptions.Center;
           
           StartCoroutine(AnimateAmmoAlert(alert, tmp));
       }
   }

   private System.Collections.IEnumerator AnimateAmmoAlert(GameObject alertObj, TextMeshProUGUI tmp)
   {
       float duration = 1.5f;
       float elapsed = 0f;
       Vector2 startPos = alertObj.GetComponent<RectTransform>().anchoredPosition;
       
       while (elapsed < duration)
       {
           elapsed += Time.unscaledDeltaTime;
           float t = elapsed / duration;
           
           alertObj.GetComponent<RectTransform>().anchoredPosition = startPos + new Vector2(0, Mathf.Lerp(0, 30f, t));

           if (t > 0.5f)
           {
               Color c = tmp.color;
               c.a = 1f - ((t - 0.5f) * 2f);
               tmp.color = c;
           }
           
           if (tmp != null) yield return null;
           else break;
       }
       
       if (alertObj != null) Destroy(alertObj);
   }

   public void ShowBuildingDamageAlert()
   {
       if (buildingDamageAlertSound != null && Code.Scripts.Audio.AudioManager.HasInstance)
       {
           Code.Scripts.EventSystems.EventManager.Instance?.Publish(new AudioClipEvent { Clip = buildingDamageAlertSound, Channel = AudioChannel.Sfx, Volume = 1f, Duration = 0f });
       }

       GameObject canvas = GameObject.Find("Canvas");
       if (canvas != null)
       {
           GameObject alert = new GameObject("BuildingDamageAlert", typeof(RectTransform), typeof(TextMeshProUGUI));
           alert.transform.SetParent(canvas.transform, false);
           
           RectTransform rt = alert.GetComponent<RectTransform>();
           rt.anchorMin = new Vector2(0.5f, 0.8f);
           rt.anchorMax = new Vector2(0.5f, 0.8f);
           rt.pivot = new Vector2(0.5f, 0.5f);
           rt.anchoredPosition = Vector2.zero;
           
           TextMeshProUGUI tmp = alert.GetComponent<TextMeshProUGUI>();
           tmp.text = "[!] BUILDINGS UNDER ATTACK! [!]";
           tmp.color = Color.red;
           tmp.fontSize = 42;
           tmp.fontStyle = FontStyles.Bold;
           tmp.alignment = TextAlignmentOptions.Center;
           
           StartCoroutine(AnimateDamageAlert(alert, tmp));
       }
   }

   private System.Collections.IEnumerator AnimateDamageAlert(GameObject alertObj, TextMeshProUGUI tmp)
   {
       float duration = 4f;
       float elapsed = 0f;
       
       while (elapsed < duration)
       {
           elapsed += Time.unscaledDeltaTime;
           
           float pingPong = Mathf.PingPong(elapsed * 5f, 1f);
           tmp.color = Color.Lerp(Color.red, new Color(1f, 0.5f, 0f), pingPong);
           
           if (elapsed > duration - 1f)
           {
               Color c = tmp.color;
               c.a = 1f - (elapsed - (duration - 1f));
               tmp.color = c;
           }
           
           if (tmp != null) yield return null;
           else break;
       }
       
       if (alertObj != null) Destroy(alertObj);
    }

    public void ShowGeneralAlert(string message, Color color)
    {
        GameObject canvas = GameObject.Find("HUD Canvas");
        if (canvas == null) canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            GameObject alert = new GameObject("GeneralAlert", typeof(RectTransform), typeof(TextMeshProUGUI));
            alert.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = alert.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.65f);
            rt.anchorMax = new Vector2(0.5f, 0.65f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI tmp = alert.GetComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.color = color;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            
            StartCoroutine(AnimateGeneralAlert(alert, tmp));
        }
    }

    private System.Collections.IEnumerator AnimateGeneralAlert(GameObject alertObj, TextMeshProUGUI tmp)
    {
        float duration = 2.0f;
        float elapsed = 0f;
        Vector2 startPos = alertObj.GetComponent<RectTransform>().anchoredPosition;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            
            if (alertObj != null)
            {
                alertObj.GetComponent<RectTransform>().anchoredPosition = startPos + new Vector2(0, Mathf.Lerp(0, 40f, t));
            }

            if (tmp != null)
            {
                if (t > 0.6f)
                {
                    Color c = tmp.color;
                    c.a = 1f - ((t - 0.6f) * 2.5f);
                    tmp.color = c;
                }
                yield return null;
            }
            else
            {
                break;
            }
        }
        
        if (alertObj != null) Destroy(alertObj);
    }
}
