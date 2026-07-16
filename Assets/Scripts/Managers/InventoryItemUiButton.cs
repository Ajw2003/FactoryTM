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
    using Buildings;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class UiItemButton : MonoBehaviour
    {
        public BuildingData buildingData;
        public TMP_Text priceText;
        public PlacementManager placementManager;
    
        public TMP_Text countText;
    
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
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.onInventoryChange += RefreshUI;
            }
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.onCurrencyChange += RefreshUI;
            }

        }
    
        private void OnDisable()
        {
            if (InventoryManager.HasInstance)
            {
                InventoryManager.Instance.onInventoryChange -= RefreshUI;
            }
            if (CurrencyManager.HasInstance)
            {
                CurrencyManager.Instance.onCurrencyChange -= RefreshUI;
            }

        }
    
        private void Start()
        {
            RefreshUI();
        }
    
        public void RefreshUI()
        {
            if (buildingData != null)
            {
                bool isUnlocked = buildingData.IsUnlocked();
    
                if (priceText != null)
                {
                    if (isUnlocked)
                    {
                        var adjustedPrice = buildingData.cost;
                        priceText.text = "$" + adjustedPrice;
                    }
                    else
                    {
                        string reqs = buildingData.GetUnlockRequirementsText();
                        string firstReq = reqs.Contains("\n") ? reqs.Split('\n')[0] : reqs;
                        priceText.text = "LOCKED\n(" + firstReq + ")";
                    }
                }
    
                if (countText != null)
                {
                    if (isUnlocked)
                    {
                        InventoryManager.InventoryItem item = InventoryManager.Instance.items.Find(i => i.data == buildingData);
                        countText.text = item != null ? item.count.ToString() : "0";
                    }
                    else
                    {
                        countText.text = "";
                    }
                }
    
                UpdateAffordabilityColor();
            }
        }
    
        private void UpdateAffordabilityColor()
        {
            Image img = GetComponent<Image>();
            if (img == null || buildingData == null || CurrencyManager.Instance == null) return;
    
            if (!buildingData.IsUnlocked())
            {
                img.color = new Color(0.25f, 0.25f, 0.25f, 0.6f);
                return;
            }
    
            Color targetColor = GetTargetColor();
            if (feedbackCoroutine == null)
            {
                img.color = targetColor;
            }
        }
    
        public Color GetTargetColor()
        {
            if (buildingData == null || CurrencyManager.Instance == null) return originalColor;
            
            if (!buildingData.IsUnlocked())
            {
                return new Color(0.02f, 0.05f, 0.02f, 0.6f);
            }
    
            // Terminal style: Dark green
            return new Color(0.05f, 0.15f, 0.05f, 0.85f);
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
    
        // This is now purely for the STORE
        public void PurchaseBuilding()
        {
            if (buildingData == null) return;
    
            if (!buildingData.IsUnlocked())
            {
                Debug.Log(buildingData.buildingName + " is locked!");
                StopActiveCoroutine();
                feedbackCoroutine = StartCoroutine(ShakeAndRedFlash());
                return;
            }
    
            float cost = buildingData.cost;
            if (CurrencyManager.Instance.currentCurrencyValue >= cost)
            {
                CurrencyManager.Instance.RemoveCurrency(buildingData.cost);
                InventoryManager.Instance.AddBuilding(buildingData, 1);
                
                // Auto-assign to first empty hotbar slot if it's the first time buying
                bool alreadyInHotbar = false;
                int emptySlot = -1;
                for (int i = 0; i < HotbarManager.Instance.slotCount; i++)
                {
                    if (HotbarManager.Instance.Slots[i] == buildingData) alreadyInHotbar = true;
                    if (emptySlot == -1 && HotbarManager.Instance.Slots[i] == null) emptySlot = i;
                }
    
                if (!alreadyInHotbar && emptySlot != -1)
                {
                    HotbarManager.Instance.AssignToSlot(emptySlot, buildingData);
                }
    
                RefreshUI();
    
                // Visual feedback for successful purchase
                StopActiveCoroutine();
                feedbackCoroutine = StartCoroutine(PunchScaleAndColor());
            }
            else
            {
                Debug.Log("Not enough currency to buy " + buildingData.buildingName);
                // Visual feedback for failed purchase (insufficient funds)
                StopActiveCoroutine();
                feedbackCoroutine = StartCoroutine(ShakeAndRedFlash());
            }
        }
    
        private System.Collections.IEnumerator PunchScaleAndColor()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            
            Image img = GetComponent<Image>();
            Color startColor = img != null ? img.color : originalColor;
            Color flashColor = new Color(0.7f, 1f, 0.7f, 1f); // soft mint green highlight
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                
                // Scale bounce
                float scaleFactor = 1f;
                if (t < 0.2f)
                    scaleFactor = Mathf.Lerp(1f, 0.88f, t / 0.2f);
                else if (t < 0.5f)
                    scaleFactor = Mathf.Lerp(0.88f, 1.12f, (t - 0.2f) / 0.3f);
                else
                    scaleFactor = Mathf.Lerp(1.12f, 1f, (t - 0.5f) / 0.5f);
                
                transform.localScale = originalScale * scaleFactor;
                
                // Color flash interpolation
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
            Color failColor = new Color(0.8f, 0.1f, 0.1f, 1f); // vibrant red flash
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                
                // Rock back and forth on Z axis
                float shakeAngle = Mathf.Sin(t * Mathf.PI * 6f) * 6f * (1f - t); 
                transform.localRotation = Quaternion.Euler(0f, 0f, shakeAngle);
                
                // Color flash
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
}


