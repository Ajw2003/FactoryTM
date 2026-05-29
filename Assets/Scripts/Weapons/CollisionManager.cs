using System;
using System.Collections.Generic;
using Singleton;
using UnityEngine;

public class CollisionManager : SingletonBase<CollisionManager>
{
    public List<BaseProjectile> playerBullets = new List<BaseProjectile>();
    public List<CartelMember> enemies = new List<CartelMember>();
    private PlayerController player;

    void Update()
    {
        // Iterate backward if you plan to destroy objects upon hit
        for (int i = playerBullets.Count - 1; i >= 0; i--)
        {
            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                if (playerBullets[i].IsCollidingWith(enemies[j]))
                {
                    // Handle Hit!
                    enemies[i].health.TakeDamage(playerBullets[i].damage);
                    Debug.Log("Hit detected!");
                    playerBullets[i].Despawn();

                    // Remove from lists, pool them, or destroy them
                    // playerBullets.RemoveAt(i);
                    // break; // Break out of inner loop since bullet is gone
                }
            }

            if (playerBullets[i].IsCollidingWith(player))
            {
                player.health.TakeDamage(playerBullets[i].damage);
            }
        }
    }

    private void Start()
    {
        player = GameManager.Instance.player;
    }

    public void DeregisterBullet(BaseProjectile bullet)
    {
        FastRemove(playerBullets, bullet);
    }

    public void DeregisterEnemy(CartelMember enemy)
    {
        FastRemove(enemies, enemy);
    }

    // O(1) List Removal Trick
    private void FastRemove<T>(List<T> list, T item)
    {
        int index = list.IndexOf(item);
        if (index == -1) return; // Item not found

        int lastIndex = list.Count - 1;
        
        // Swap the item with the last item in the list
        list[index] = list[lastIndex];
        
        // Remove the last item (no memory shifting required!)
        list.RemoveAt(lastIndex);
    }
    
    public void RegisterBullet(BaseProjectile bullet) => playerBullets.Add(bullet);
    public void RegisterEnemy(CartelMember enemy) => enemies.Add(enemy);
}