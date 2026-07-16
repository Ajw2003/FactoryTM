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
    using System.Collections;
    using System.Collections.Generic;
    using Singleton;
    using UnityEngine;
    using Random = UnityEngine.Random;
    
    public enum RaidMode { WaveBased, Continuous }
    
    [System.Serializable]
    public class RaidSettings
    {
        public string raidName = "Standard Raid";
        public RaidMode mode = RaidMode.WaveBased;
        
        [Header("Wave Settings")]
        public int totalWaves = 3;
        public int enemiesPerWave = 5;
        public float waveFrequency = 15f; // Time between waves
        
        [Header("Continuous Settings")]
        public float raidDuration = 60f; // Total length of the raid in seconds
        public float spawnInterval = 1f; // Time between each enemy spawn
        
        [Header("General Settings")]
        public List<GameObject> enemyPrefabs;
        public float spawnDistance = 20f; // Distance from player/center to spawn
        
        // Allow overriding some settings for quick calls
        public RaidSettings Clone()
        {
            return (RaidSettings)this.MemberwiseClone();
        }
    }
    
    public class RaidManager : SingletonBase<RaidManager>
    {
        [Header("Default Settings")]
        public RaidSettings defaultSettings;
    
        [Header("Status")]
        public bool isRaidActive = false;
        public int currentWave = 0;
        public float remainingDuration = 0f;
    
        private Coroutine raidCoroutine;
    
        protected override void Awake()
        {
            persistBetweenScenes = false;
            base.Awake();
        }
    
        /// <summary>
        /// Starts a raid with default settings.
        /// </summary>
        [Button("Start Default Raid")]
        public void StartDefaultRaid()
        {
            StartRaid(defaultSettings);
        }
    
        /// <summary>
        /// Public function to spawn a raid with set parameters.
        /// </summary>
        public void StartCustomRaid(int waves, int enemiesPerWave, float frequency, float duration = 0, RaidMode mode = RaidMode.WaveBased)
        {
            RaidSettings custom = defaultSettings.Clone();
            custom.totalWaves = waves;
            custom.enemiesPerWave = enemiesPerWave;
            custom.waveFrequency = frequency;
            custom.raidDuration = duration;
            custom.mode = mode;
            
            StartRaid(custom);
        }
    
        /// <summary>
        /// Starts a raid with a full settings object.
        /// </summary>
        public void StartRaid(RaidSettings settings)
        {
            if (isRaidActive)
            {
                Debug.LogWarning("A raid is already active!");
                return;
            }
    
            if (settings.enemyPrefabs == null || settings.enemyPrefabs.Count == 0)
            {
                Debug.LogError("No enemy prefabs assigned for the raid!");
                return;
            }
    
            raidCoroutine = StartCoroutine(RaidRoutine(settings));
        }
    
        private IEnumerator RaidRoutine(RaidSettings settings)
        {
            isRaidActive = true;
            Debug.Log($"Raid Started: {settings.raidName} (Mode: {settings.mode})");
    
            if (settings.mode == RaidMode.WaveBased)
            {
                yield return WaveBasedRaid(settings);
            }
            else
            {
                yield return ContinuousRaid(settings);
            }
    
            isRaidActive = false;
            Debug.Log("Raid Finished!");
            raidCoroutine = null;
        }
    
        private IEnumerator WaveBasedRaid(RaidSettings settings)
        {
            currentWave = 0;
            while (currentWave < settings.totalWaves)
            {
                currentWave++;
                Debug.Log($"Wave {currentWave}/{settings.totalWaves} starting!");
    
                for (int i = 0; i < settings.enemiesPerWave; i++)
                {
                    SpawnEnemy(settings.enemyPrefabs, settings.spawnDistance);
                    yield return new WaitForSecondsRealtime(settings.spawnInterval);
                }
    
                if (currentWave < settings.totalWaves)
                {
                    Debug.Log($"Wave {currentWave} finished. Next wave in {settings.waveFrequency} seconds.");
                    yield return new WaitForSecondsRealtime(settings.waveFrequency);
                }
            }
        }
    
        private IEnumerator ContinuousRaid(RaidSettings settings)
        {
            remainingDuration = settings.raidDuration;
            while (remainingDuration > 0)
            {
                SpawnEnemy(settings.enemyPrefabs, settings.spawnDistance);
                float waitTime = settings.spawnInterval;
                yield return new WaitForSecondsRealtime(waitTime);
                remainingDuration -= waitTime;
            }
        }
    
        private void SpawnEnemy(List<GameObject> prefabs, float distance)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            Vector3 spawnPos = GetRandomSpawnPosition(distance);
    
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            CartelMember member = enemy.GetComponent<CartelMember>();
            if (member != null)
            {
                member.isRaidEnemy = true;
            }
        }
    
        private Vector3 GetRandomSpawnPosition(float distance)
        {
            Vector3 center = Vector3.zero;
    
            if (GameManager.Instance.playerController != null)
            {
                center = GameManager.Instance.playerController.transform.position;
            }
            else if (GridManager.Instance != null)
            {
                Vector2Int centerCell = GridManager.Instance.center;
                center = GridManager.Instance.CellToWorldConversion(centerCell);
            }
    
            float angle = Random.Range(0f, 2f * Mathf.PI);
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * distance;
            return center + offset;
        }
    
        [Button("Stop Raid")]
        public void StopRaid()
        {
            if (raidCoroutine != null)
            {
                StopCoroutine(raidCoroutine);
                raidCoroutine = null;
            }
            isRaidActive = false;
            Debug.Log("Raid manually stopped.");
        }
    }
    
}


