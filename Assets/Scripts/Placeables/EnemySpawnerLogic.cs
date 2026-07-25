using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Placeables
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public class EnemySpawnerLogic : BuildingLogic
    {
        public GameObject enemyPrefab;
        public int spawnLimit = 5; // 0 for infinite/repeatable
        public float spawnCooldown = 8f;
        public int maxConcurrentEnemies = 3;
        public float activationRange = 9f; // Reduced from 15f to prevent off-screen activation
    
        private List<CartelMember> spawnedEnemies = new List<CartelMember>();
        private float lastSpawnTime = -99f;
        private bool isActivated = false;
        private int totalSpawnedCount = 0;
    
        public override void Setup(Buildings.BuildingData buildingData, Vector2Int cell)
        {
            base.Setup(buildingData, cell);
            Health = data.maxHealth;
    
            if (data != null)
            {
                spawnLimit = data.spawnLimit;
                spawnCooldown = data.spawnCooldown;
                maxConcurrentEnemies = data.maxConcurrentEnemies;
                activationRange = data.activationRange;
            }
    
            if (enemyPrefab == null)
            {
                enemyPrefab = Resources.Load<GameObject>("prefabs/Npcs/Enemy");
            }
        }
    
        public override void PerformAction()
        {
            // Safety check: do not spawn if tutorial is active but we haven't reached the outpost destruction phase yet
            if (TutorialManager.HasInstance && !TutorialManager.Instance.IsTutorialCompleted())
            {
                if (TutorialManager.Instance.currentState < TutorialState.DestroyEnemyOutpost)
                {
                    return;
                }
            }
    
            // If claimed or deactivated, do not spawn enemies
            if (!IsEnemyOwned)
            {
                return;
            }
    
            // Clean up dead enemies
            spawnedEnemies.RemoveAll(e => e == null);
    
            // Check activation range
            if (!isActivated && PlayerController.Instance != null)
            {
                float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
                if (dist <= activationRange)
                {
                    isActivated = true;
                    Debug.Log($"Enemy Spawner at {myCell} activated!");
                }
            }
    
            // Spawning cycle
            if (isActivated)
            {
                if (spawnLimit > 0 && totalSpawnedCount >= spawnLimit)
                {
                    return; // Reached total limit
                }
    
                if (spawnedEnemies.Count >= maxConcurrentEnemies)
                {
                    return; // Reached concurrent limit
                }
    
                if (Time.time - lastSpawnTime >= spawnCooldown)
                {
                    lastSpawnTime = Time.time;
                    SpawnEnemy();
                }
            }
        }
    
        private void SpawnEnemy()
        {
            if (enemyPrefab == null)
            {
                enemyPrefab = Resources.Load<GameObject>("prefabs/Npcs/Enemy");
            }
    
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemySpawnerLogic: Enemy prefab is null and could not be loaded from resources.");
                return;
            }
    
            // Spawn slightly offset from the spawner position to prevent overlapping physics issues
            Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            GameObject enemyGO = Instantiate(enemyPrefab, transform.position + spawnOffset, Quaternion.identity);
            
            CartelMember enemy = enemyGO.GetComponent<CartelMember>();
            if (enemy != null)
            {
                enemy.isRaidEnemy = false;
                spawnedEnemies.Add(enemy);
                
                if (outpost != null)
                {
                    outpost.RegisterSpawnedEnemy(enemy);
                }
                totalSpawnedCount++;
                
                // Spawn float text
                FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/EnemySpawnAlertSettings");
                FloatingTextManager.Instance.Spawn("ALERT!", transform.position, settings);
            }
            else
            {
                Debug.LogError("EnemySpawnerLogic: Spawned prefab does not have CartelMember component!");
                Destroy(enemyGO);
            }
        }
    
        public int GetActiveSpawnedCount()
        {
            spawnedEnemies.RemoveAll(e => e == null);
            return spawnedEnemies.Count;
        }
    
        public bool HasFinishedSpawning()
        {
            if (spawnLimit > 0 && totalSpawnedCount >= spawnLimit)
            {
                spawnedEnemies.RemoveAll(e => e == null);
                return spawnedEnemies.Count == 0;
            }
            return false;
        }
    }
    
}


