using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Ui
{
    using Managers;
    using TMPro;
    using UnityEngine;
    
    public class StatsUiScript : MonoBehaviour
    {
        private TMP_Text moneyEarnedText;
        private TMP_Text totalKillsText;
        private TMP_Text upgradesPurchasedText;
        private TMP_Text daysSurvivedText;
        private TMP_Text oreMinedText;
    
        private void Awake()
        {
            // Try to find text components, or dynamically generate them if they don't exist
            Transform contentTransform = transform.Find("Content");
            if (contentTransform == null) contentTransform = transform.Find("Scroll View/Viewport/Content");
            if (contentTransform == null) contentTransform = transform; // Fallback to this transform
    
            moneyEarnedText = FindOrCreateText(contentTransform, "MoneyEarnedText", "Money Earned: $0", 0);
            totalKillsText = FindOrCreateText(contentTransform, "TotalKillsText", "Total Kills: 0", 1);
            upgradesPurchasedText = FindOrCreateText(contentTransform, "UpgradesPurchasedText", "Upgrades Purchased: 0", 2);
            daysSurvivedText = FindOrCreateText(contentTransform, "DaysSurvivedText", "Days Survived: 0", 3);
            oreMinedText = FindOrCreateText(contentTransform, "OreMinedText", "Ore Mined: 0", 4);
        }
    
        private void OnEnable()
        {
            RefreshStats();
        }
    
        private void Update()
        {
            RefreshStats();
        }
    
        private TMP_Text FindOrCreateText(Transform parent, string name, string defaultText, int index)
        {
            Transform child = parent.Find(name);
            if (child != null)
            {
                return child.GetComponent<TMP_Text>();
            }
    
            // If not found, let's create a simple text object
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
    
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -50 - (index * 60)); // Stack vertically
            rt.sizeDelta = new Vector2(600, 50);
    
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.2f, 1f, 0.2f, 1f);
            
            return tmp;
        }
    
        public void RefreshStats()
        {
            if (GameStatsManager.HasInstance)
            {
                if (moneyEarnedText != null) moneyEarnedText.text = $"Money Earned: ${GameStatsManager.Instance.moneyEarnedToDate:F0}";
                if (totalKillsText != null) totalKillsText.text = $"Total Kills: {GameStatsManager.Instance.totalKills}";
                if (upgradesPurchasedText != null) upgradesPurchasedText.text = $"Upgrades Purchased: {GameStatsManager.Instance.upgradesPurchased}";
                if (daysSurvivedText != null) daysSurvivedText.text = $"Days Survived: {GameStatsManager.Instance.daysSurvived}";
                if (oreMinedText != null) oreMinedText.text = $"Ore Mined: {GameStatsManager.Instance.oreMined}";
            }
        }
    
        public void CloseStats()
        {
            gameObject.SetActive(false);
        }
    
        public void OpenStats()
        {
            gameObject.SetActive(true);
            RefreshStats();
        }
    }
    
}


