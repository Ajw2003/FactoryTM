using System.Collections;
using Managers;
using UnityEngine;
using Singleton;
using TMPro;
using UnityEngine.UI;

public enum CyclePhase
{
    Day,
    Evening,
    UpgradePhase
}

public class DayNightManager : SingletonBase<DayNightManager>
{
    [Header("Cycle Settings")]
    public float dayDuration = 15f; // Duration of day in seconds
    public int currentDay = 1;
    public CyclePhase currentPhase = CyclePhase.Day;

    [Header("Raid Settings per Day")]
    public int baseEnemiesPerWave = 3;
    public int wavesPerRaid = 2;
    public float waveInterval = 10f;

    [Header("UI Reference")]
    private TMP_Text timerText;
    private float timeRemaining;
    private bool isRaidStarted = false;
    private bool raidHasBegun = false; // True only once enemies have actually spawned
    private float raidSettleTimer = 0f;
    private const float RaidSettleDelay = 3f; // Wait at least this many seconds after raid start before checking completion

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
    }

    private void Start()
    {
        currentPhase = CyclePhase.Day;
        timeRemaining = dayDuration;
        isRaidStarted = false;
        raidHasBegun = false;
        raidSettleTimer = 0f;
        
        SetupTimerUI();
    }

    private void Update()
    {
        if (currentPhase == CyclePhase.Day)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerText($"DAY {currentDay} // SUNSET IN {Mathf.CeilToInt(timeRemaining)}");
            }
            else
            {
                StartEveningRaid();
            }
        }
        else if (currentPhase == CyclePhase.Evening)
        {
            // Use unscaled time so the settle timer is never frozen by a pause
            raidSettleTimer += Time.unscaledDeltaTime;

            int activeEnemies = GameManager.Instance != null ? GameManager.Instance.ActiveEnemies.Count : 0;
            
            // Mark raidHasBegun once enemies actually appear on screen
            if (!raidHasBegun && activeEnemies > 0)
            {
                raidHasBegun = true;
            }

            bool raidFinished = RaidManager.Instance != null && !RaidManager.Instance.isRaidActive;
            bool allClear = raidHasBegun && raidFinished && activeEnemies == 0 && raidSettleTimer >= RaidSettleDelay;

            if (allClear)
            {
                TriggerUpgradePhase();
                Debug.Log("triggerPhase");
            }
            else
            {
                UpdateTimerText($"WARNING: RAID ACTIVE // ENEMIES LEFT: {activeEnemies}", new Color(1f, 0.2f, 0.2f));
            }
        }
    }

    private void StartEveningRaid()
    {
        currentPhase = CyclePhase.Evening;
        isRaidStarted = true;
        raidHasBegun = false;
        raidSettleTimer = 0f;

        if (RaidManager.Instance != null)
        {
            // Scale raid difficulty dynamically based on current day
            int enemies = baseEnemiesPerWave + (currentDay - 1) * 2;
            int waves = wavesPerRaid + (currentDay / 3); // Extra wave every 3 days
            
            Debug.Log($"DayNightManager: Triggering evening raid for Day {currentDay} ({waves} waves, {enemies} enemies per wave)");
            RaidManager.Instance.StartCustomRaid(waves, enemies, waveInterval);
        }
    }

    private void TriggerUpgradePhase()
    {
        currentPhase = CyclePhase.UpgradePhase;
        // Pause everything immediately — upgrade selection must be timescale-independent
        Time.timeScale = 0f;
        UpdateTimerText("RAID REPELLED // RESEARCH INCOMING...", new Color(0.2f, 1f, 0.2f));

        // Open the upgrade panel
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ShowUpgradeSelection();
        }
    }

    public void StartNextDay()
    {
        currentDay++;
        currentPhase = CyclePhase.Day;
        timeRemaining = dayDuration;
        isRaidStarted = false;
        raidHasBegun = false;
        raidSettleTimer = 0f;
        Time.timeScale = 1f;

        UpdateTimerText($"DAY {currentDay} STARTED");
    }

    private void SetupTimerUI()
    {
        // Try to find if a timer text object already exists
        GameObject timerGo = GameObject.Find("DayNightTimerText");
        if (timerGo != null)
        {
            timerText = timerGo.GetComponent<TMP_Text>();
            return;
        }

        // Dynamically instantiate a retro-looking timer HUD at the top of the canvas
        GameObject hudCanvas = GameObject.Find("HUD Canvas");
        if (hudCanvas == null) hudCanvas = GameObject.Find("Canvas");
        if (hudCanvas == null) hudCanvas = FindFirstObjectByType<Canvas>()?.gameObject;

        if (hudCanvas != null)
        {
            timerGo = new GameObject("DayNightTimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
            timerGo.transform.SetParent(hudCanvas.transform, false);

            RectTransform rt = timerGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -60f);
            rt.sizeDelta = new Vector2(400f, 50f);

            timerText = timerGo.GetComponent<TextMeshProUGUI>();
            timerText.fontSize = 36;
            timerText.fontStyle = FontStyles.Bold;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = new Color(0.2f, 0.9f, 0.2f, 1f); // Retro terminal green
            
            // Add a subtle scanline overlay or shadow
            Outline outline = timerGo.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.75f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }

    private void UpdateTimerText(string text, Color? color = null)
    {
        if (timerText != null)
        {
            timerText.text = text;
            if (color.HasValue)
            {
                timerText.color = color.Value;
            }
            else
            {
                // Default retro terminal green
                timerText.color = new Color(0.2f, 0.9f, 0.2f, 1f);
            }
        }
    }
}
