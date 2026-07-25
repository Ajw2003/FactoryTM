using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Managers
{
    using System;
    using System.Collections;
    using Managers;
    using UnityEngine;
    using Singleton;
    using TMPro;
    using UnityEngine.Rendering.Universal;
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
        [SerializeField] private float dayDuration = 90f; // Duration of day in seconds
        [SerializeField] private int currentDay = 1;
        [SerializeField] private CyclePhase currentPhase = CyclePhase.Day;
        [SerializeField] private bool isTutorialActive = true;

        [Header("Raid Settings per Day")]
        [SerializeField] private int baseEnemiesPerWave = 3;
        [SerializeField] private int wavesPerRaid = 2;
        [SerializeField] private float waveInterval = 10f;
        [SerializeField] private int raidEnemyReduction = 0;

        [Header("UI Reference")]
        [SerializeField] private float timeRemaining;

        public float DayDuration => dayDuration;
        public int CurrentDay => currentDay;
        public CyclePhase CurrentPhase => currentPhase;
        public bool IsTutorialActive => isTutorialActive;
        public int RaidEnemyReduction => raidEnemyReduction;
        public float TimeRemaining => timeRemaining;

        /// <summary>Suspends/resumes day-cycle progression while the tutorial drives pacing.</summary>
        public void SetTutorialActive(bool value)
        {
            isTutorialActive = value;
        }

        /// <summary>Overrides the countdown to the next phase, used by the tutorial to pace transitions.</summary>
        public void SetTimeRemaining(float seconds)
        {
            timeRemaining = seconds;
        }

        /// <summary>Permanently reduces how many enemies each raid wave spawns.</summary>
        public void AddRaidEnemyReduction(int amount)
        {
            raidEnemyReduction += amount;
        }
        private bool isRaidStarted = false;
        private bool raidHasBegun = false; // True only once enemies have actually spawned
        private float raidSettleTimer = 0f;
        private const float RaidSettleDelay = 3f; // Wait at least this many seconds after raid start before checking completion
        [SerializeField]private Light2D sun;
        [SerializeField] private Light2D nightVis;
    
        private float currentTime;
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }
    
        private void Start()
        {
            // sun = FindFirstObjectByType<Light2D>();
            currentTime = dayDuration;
            currentPhase = CyclePhase.Day;
            timeRemaining = dayDuration;
            isRaidStarted = false;
            raidHasBegun = false;
            raidSettleTimer = 0f;
            nightVis.intensity = 0f;
        }
    
        private void Update()
        {
            if (currentPhase == CyclePhase.Day)
            {
                if (isTutorialActive)
                {
                    return;
                }
    
                if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime;
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
    
                int activeEnemies = 0;
                if (GameManager.Instance != null && GameManager.Instance.ActiveEnemies != null)
                {
                    for (int i = 0; i < GameManager.Instance.ActiveEnemies.Count; i++)
                    {
                        var enemy = GameManager.Instance.ActiveEnemies[i];
                        if (enemy != null && enemy.isRaidEnemy)
                        {
                            activeEnemies++;
                        }
                    }
                }
                
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
            }
        }
    
        private void StartEveningRaid()
        {
            nightVis.intensity = 5.0f;
            currentPhase = CyclePhase.Evening;
            isRaidStarted = true;
            raidHasBegun = false;
            raidSettleTimer = 0f;
    
            if (RaidManager.Instance != null)
            {
                // Scale raid difficulty dynamically based on current day
                int enemies = Mathf.Max(1, baseEnemiesPerWave + (currentDay - 1) * 2 - raidEnemyReduction);
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
    
            // Open the upgrade panel
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.ShowUpgradeSelection();
            }
        }
    
        public void StartNextDay()
        {
            if (Managers.GameStatsManager.HasInstance)
            {
                Managers.GameStatsManager.Instance.IncrementDaysSurvived();
            }
    
            currentDay++;
            nightVis.intensity = 0f;
            currentPhase = CyclePhase.Day;
            timeRemaining = dayDuration;
            isRaidStarted = false;
            raidHasBegun = false;
            raidSettleTimer = 0f;
            Time.timeScale = 1f;
            sun.intensity = 1f;
    
        }
    
        public void CompleteTutorial()
        {
            isTutorialActive = false;
        }
    
    }
    
}


