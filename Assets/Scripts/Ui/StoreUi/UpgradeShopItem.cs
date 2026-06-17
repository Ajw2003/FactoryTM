using System.Collections;
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
    private TMP_Text priceLabel;
    private TMP_Text nameLabel;
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
        if (priceLabel != null)
            priceLabel.text = "$" + definition.costInShop;

        UpdateAffordabilityColor();
    }

    private string GetDisplayName()
    {
        switch (definition.type)
        {
            case UpgradeType.HealthPack: return "HEALTH PACK (x1)";
            case UpgradeType.Ammo: return "AMMO PACK (x30)";
            case UpgradeType.Weapon: return definition.upgradeName + "\n[WEAPON]";
            case UpgradeType.Building: return definition.buildingToUnlock != null
                ? definition.buildingToUnlock.buildingName + "\n[BUILDING]"
                : definition.upgradeName;
            case UpgradeType.Armor: return definition.upgradeName + "\n[ARMOR]";
            default: return definition.upgradeName;
        }
    }

    private void UpdateAffordabilityColor()
    {
        if (bgImage == null || !CurrencyManager.HasInstance) return;
        bgImage.color = GetTargetColor();
    }

    public Color GetTargetColor()
    {
        return new Color(0.05f, 0.15f, 0.05f, 0.85f);
    }

    public void PurchaseUpgrade()
    {
        if (definition == null) return;

        float cost = definition.costInShop;

        if (!CurrencyManager.HasInstance || CurrencyManager.Instance.currentCurrencyValue < cost)
        {
            Debug.Log($"Not enough currency to buy {definition.upgradeName}");
            if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = StartCoroutine(ShakeAndRedFlash());
            return;
        }

        CurrencyManager.Instance.RemoveCurrency(definition.costInShop);

        // Apply the effect
        switch (definition.type)
        {
            case UpgradeType.HealthPack:
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.healthPacksCount += 1;
                    Debug.Log($"Purchased health pack. Total: {PlayerController.Instance.healthPacksCount}");
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
                }
                break;

            case UpgradeType.Armor:
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.damageReductionFactor = Mathf.Clamp01(
                        PlayerController.Instance.damageReductionFactor + definition.armorPercentBoost * 0.5f); // Half bonus on re-buy
                    PlayerController.Instance.maxHealth += Mathf.RoundToInt(definition.maxHealthBoost * 0.5f);
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
                        playerWeapon.SendMessage("Start", SendMessageOptions.DontRequireReceiver);
                    }
                }
                break;

            case UpgradeType.Building:
                // Buildings are just unlocked in the store; player buys them through the normal building buttons
                Debug.Log($"Building {definition.buildingToUnlock?.buildingName} already unlocked — no action needed.");
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
