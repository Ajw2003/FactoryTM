using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
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
        public float dayDuration = 90f; // Duration of day in seconds
        public int currentDay = 1;
        public CyclePhase currentPhase = CyclePhase.Day;
        public bool isTutorialActive = true;
    
        [Header("Raid Settings per Day")]
        public int baseEnemiesPerWave = 3;
        public int wavesPerRaid = 2;
        public float waveInterval = 10f;
        public int raidEnemyReduction = 0;
    
        [Header("UI Reference")]
        public float timeRemaining;
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


