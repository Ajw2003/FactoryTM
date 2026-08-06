using System.Collections;
using System.Collections.Generic;
using Code.Scripts.EventSystems;
using EventTypes;
using Managers;
using Placeables;
using Ui;
using UnityEngine;
using Weapons;

public class StoryNpc : MonoBehaviour, IHealth
{

    [SerializeField] private float speed;

    [SerializeField] private float turnSpeed;
    
    [SerializeField] private float sightRange = 12f;
    

    [SerializeField] private int profitFromKill;

    public bool collided;
    
    
    public int Health { get; private set; }
    public int MaxHealth = 10;
    
    
    public float width = 1.0f;
    public float height = 1.0f;
    
    public DialogueSO dialogue;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = MaxHealth;
        EventManager.Instance?.Subscribe(this, (NpcDialogueFinishedEvent e) => DialogueFinished(e.Dialogue));
        
    }

    private void DialogueFinished(DialogueSO Dialogue)
    {
        if(Dialogue != dialogue) return;
        collided = false;
    }

    public void Collide()
    {
        collided = true;
        EventManager.Instance?.Publish(new NpcDialogueEvent{Dialogue =  dialogue,});
    }
    
    void OnEnable()
    {
        GameManager.Instance?.ActiveStoryNpcs.Add(this);
    }

    // Remove from the list when destroyed
    void OnDisable()
    {
        if (GameManager.HasInstance) 
        {
            GameManager.Instance.ActiveStoryNpcs.Remove(this);
        }
    }
   
    public Rectangle2D GetBoundingBox()
    {
        float angleRadians = transform.eulerAngles.z * Mathf.Deg2Rad;
        return TwoDCollision.CreateFromRotated(
            transform.position.x, transform.position.y, width, height, angleRadians);
    }

    
    public void TakeDamage(int amount)
    {
        Health -= amount;

        // Spawn floating damage text in bright yellow-orange
        FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/CartelDamageSettings");
        FloatingTextManager.Instance.Spawn(amount.ToString(), transform.position, settings);

        // Visual flash feedback
        StartCoroutine(FlashRed());

        if (Health <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(0.12f);
            sprite.color = originalColor;
        }
    }

    public void Die()
    {
        if (GameStatsManager.HasInstance)
        {
            GameStatsManager.Instance.IncrementKills();
        }
        CurrencyManager.Instance.AddCurrency(profitFromKill);
        
        FloatingTextSettings settings = Resources.Load<FloatingTextSettings>("FloatingTextSettings/CartelKillRewardSettings");
        FloatingTextManager.Instance.Spawn(profitFromKill.ToString(), transform.position, settings);

        Destroy(this.gameObject);
    }
}
