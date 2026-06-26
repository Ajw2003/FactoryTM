using System.Collections;
using Buildings;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles purchasing a researched upgrade from the shop (health packs, weapons, etc).
/// Attach to dynamically-generated shop buttons in StoreUiScript.
/// </summary>
public class UpgradeShopItem : MonoBehaviour
{
    private UpgradeDefinition definition;
    public UpgradeDefinition Definition => definition;
    private TMP_Text priceLabel;
    private TMP_Text nameLabel;
    private TMP_Text descLabel;
    private TMP_Text typeLabel;
    private Image bgImage;
    private Vector3 originalScale;
    private Coroutine feedbackCoroutine;

    public void Setup(UpgradeDefinition def)
    {
        definition = def;
        originalScale = transform.localScale;
        bgImage = GetComponent<Image>();

        // Find or create name/price labels
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();
        foreach (var t in texts)
        {
            if (t.gameObject.name.Contains("Name")) nameLabel = t;
            else if (t.gameObject.name.Contains("Price")) priceLabel = t;
            else if (t.gameObject.name.Contains("Desc")) descLabel = t;
            else if (t.gameObject.name.Contains("Type")) typeLabel = t;
        }

        RefreshUI();

        // Subscribe to upgrade changes so the button can refresh
        if (UpgradeManager.HasInstance)
        {
            UpgradeManager.Instance.onUpgradesChanged += RefreshUI;
        }
    }

    private void OnDestroy()
    {
        if (UpgradeManager.HasInstance)
        {
            UpgradeManager.Instance.onUpgradesChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        if (definition == null) return;

        if (nameLabel != null)
            nameLabel.text = GetDisplayName();

        if (descLabel != null)
            descLabel.text = definition.description;

        if (typeLabel != null)
            typeLabel.text = "[" + GetTypeString() + "]";

        if (priceLabel != null)
        {
            if (definition.type == UpgradeType.ZoneExpansion && ZoneManager.HasInstance)
            {
                priceLabel.text = "$" + ZoneManager.Instance.GetUnlockCost().ToString("F0");
            }
            else
            {
                priceLabel.text = "$" + definition.costInShop;
            }
        }
    }

    private string GetDisplayName()
    {
        switch (definition.type)
        {
            case UpgradeType.HealthPack: return "HEALTH PACK (x1)";
            case UpgradeType.Ammo: return "AMMO PACK (x30)";
            case UpgradeType.Building: return definition.buildingToUnlock != null
                ? definition.buildingToUnlock.buildingName
                : definition.upgradeName;
            default: return definition.upgradeName;
        }
    }

    public string GetTypeString()
    {
        switch (definition.type)
        {
            case UpgradeType.HealthPack: return "CONSUMABLE";
            case UpgradeType.Ammo: return "CONSUMABLE";
            case UpgradeType.Weapon: return "WEAPON";
            case UpgradeType.Building: return "BUILDING";
            case UpgradeType.Armor: return "ARMOR";
            case UpgradeType.ZoneExpansion: return "EXPANSION";
            default: return "UPGRADE";
        }
    }



    public Color GetTargetColor()
    {
        return new Color(0.05f, 0.15f, 0.05f, 0.85f);
    }

    public void PurchaseUpgrade()
    {
        if (definition == null) return;

        float cost = definition.type == UpgradeType.ZoneExpansion && ZoneManager.HasInstance 
            ? ZoneManager.Instance.GetUnlockCost() 
            : definition.costInShop;

        if (!CurrencyManager.HasInstance || CurrencyManager.Instance.currentCurrencyValue < cost)
        {
            Debug.Log($"Not enough currency to buy {definition.upgradeName}");
            if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = StartCoroutine(ShakeAndRedFlash());
            return;
        }

        CurrencyManager.Instance.RemoveCurrency(cost);

        if (Managers.GameStatsManager.HasInstance)
        {
            Managers.GameStatsManager.Instance.IncrementUpgradesPurchased();
        }

        // Apply the effect
        switch (definition.type)
        {
            case UpgradeType.HealthPack:
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.healthPacksCount += 1;
                    Debug.Log($"Purchased health pack. Total: {PlayerController.Instance.healthPacksCount}");
                    
                    if (TutorialManager.HasInstance)
                    {
                        TutorialManager.Instance.HandleHealthPackPurchased();
                    }
                }
                break;

            case UpgradeType.Ammo:
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.ammoReserve += 30;
                    Debug.Log($"Purchased ammo pack. Total: {PlayerController.Instance.ammoReserve}");
                    
                    PlayerWeapon playerWeapon = FindFirstObjectByType<PlayerWeapon>();
                    if (playerWeapon != null)
                    {
                        playerWeapon.UpdateAmmoUI();
                    }

                    if (TutorialManager.HasInstance)
                    {
                        TutorialManager.Instance.HandleAmmoPurchased();
                    }
                }
                break;

            case UpgradeType.Armor:
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.damageReductionFactor = Mathf.Clamp01(PlayerController.Instance.damageReductionFactor + definition.armorPercentBoost * 0.5f); // Half bonus on re-buy
                    int healthBoost = Mathf.RoundToInt(definition.maxHealthBoost * 0.5f);
                    PlayerController.Instance.maxHealth += healthBoost;
                    PlayerController.Instance.Health = Mathf.Min(PlayerController.Instance.maxHealth, PlayerController.Instance.Health + healthBoost);
                    UiManager.Instance.UpdateHp(PlayerController.Instance.Health, PlayerController.Instance.maxHealth);
                }
                break;

            case UpgradeType.Weapon:
                // Weapon upgrades are one-time only; buying from the store swaps weapons
                if (definition.weaponToUnlock != null)
                {
                    PlayerWeapon playerWeapon = FindFirstObjectByType<PlayerWeapon>();
                    if (playerWeapon != null)
                    {
                        playerWeapon.Stats = definition.weaponToUnlock;
                        playerWeapon.ApplyStats();
                    }

                    if (TutorialManager.HasInstance)
                    {
                        TutorialManager.Instance.HandleWeaponPurchased(definition);
                    }
                }
                break;

            case UpgradeType.Building:
                if (InventoryManager.HasInstance && definition.buildingToUnlock != null)
                {
                    int amount = 1;
                    if (definition.buildingToUnlock.type == BuildingType.Conveyor && TutorialManager.HasInstance && TutorialManager.Instance.currentState == TutorialState.BuyMinerConveyors)
                    {
                        amount = 10;
                    }
                    InventoryManager.Instance.AddBuilding(definition.buildingToUnlock, amount);
                    Debug.Log($"Purchased building: {definition.buildingToUnlock.buildingName} x{amount}");
                    
                    // Auto-assign to first empty hotbar slot
                    bool alreadyInHotbar = false;
                    int emptySlot = -1;
                    if (HotbarManager.HasInstance)
                    {
                        for (int i = 0; i < HotbarManager.Instance.slotCount; i++)
                        {
                            if (HotbarManager.Instance.Slots[i] == definition.buildingToUnlock) alreadyInHotbar = true;
                            if (emptySlot == -1 && HotbarManager.Instance.Slots[i] == null) emptySlot = i;
                        }

                        if (!alreadyInHotbar && emptySlot != -1)
                        {
                            HotbarManager.Instance.AssignToSlot(emptySlot, definition.buildingToUnlock);
                        }
                    }
                }
                break;
        }

        RefreshUI();
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(PunchScaleAndColor());
    }

    private IEnumerator PunchScaleAndColor()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Color startColor = bgImage != null ? bgImage.color : Color.white;
        Color flashColor = new Color(0.7f, 1f, 0.7f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float s = t < 0.5f ? Mathf.Lerp(1f, 1.12f, t / 0.5f) : Mathf.Lerp(1.12f, 1f, (t - 0.5f) / 0.5f);
            transform.localScale = originalScale * s;
            if (bgImage != null)
                bgImage.color = t < 0.3f
                    ? Color.Lerp(startColor, flashColor, t / 0.3f)
                    : Color.Lerp(flashColor, GetTargetColor(), (t - 0.3f) / 0.7f);
            yield return null;
        }
        transform.localScale = originalScale;
        if (bgImage != null) bgImage.color = GetTargetColor();
        feedbackCoroutine = null;
    }

    private IEnumerator ShakeAndRedFlash()
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Color startColor = bgImage != null ? bgImage.color : Color.white;
        Color failColor = new Color(0.8f, 0.1f, 0.1f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float shakeAngle = Mathf.Sin(t * Mathf.PI * 6f) * 6f * (1f - t);
            transform.localRotation = Quaternion.Euler(0f, 0f, shakeAngle);
            if (bgImage != null)
                bgImage.color = t < 0.2f
                    ? Color.Lerp(startColor, failColor, t / 0.2f)
                    : Color.Lerp(failColor, GetTargetColor(), (t - 0.2f) / 0.8f);
            yield return null;
        }
        transform.localRotation = Quaternion.identity;
        if (bgImage != null) bgImage.color = GetTargetColor();
        feedbackCoroutine = null;
    }
}
