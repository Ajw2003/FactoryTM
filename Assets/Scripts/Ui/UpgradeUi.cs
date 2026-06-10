using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Singleton;
using DG.Tweening;
using Managers;
using UnityEngine.EventSystems;

public class UpgradeUi : SingletonBase<UpgradeUi>
{
    private GameObject panelGo;
    private CanvasGroup canvasGroup;
    private List<GameObject> activeCards = new List<GameObject>();

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
        
        CreateUpgradePanelHierarchy();
    }

    private void CreateUpgradePanelHierarchy()
    {
        // Find main HUD canvas
        GameObject canvasGo = GameObject.Find("HUD Canvas");
        if (canvasGo == null) canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null) canvasGo = FindFirstObjectByType<Canvas>()?.gameObject;

        if (canvasGo == null)
        {
            Debug.LogError("UpgradeUi: Canvas not found!");
            return;
        }

        // Create main background panel
        panelGo = new GameObject("UpgradeSelectionPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panelGo.transform.SetParent(canvasGo.transform, false);

        RectTransform panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        Image panelImg = panelGo.GetComponent<Image>();
        panelImg.color = new Color(0.01f, 0.05f, 0.01f, 0.96f); // Dark retro CRT tint

        canvasGroup = panelGo.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        panelGo.SetActive(false);

        // Header Title
        GameObject titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(panelGo.transform, false);

        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 0.85f);
        titleRt.anchorMax = new Vector2(0.5f, 0.85f);
        titleRt.pivot = new Vector2(0.5f, 0.5f);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta = new Vector2(600f, 60f);

        TextMeshProUGUI titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
        titleTxt.text = "CHOOSE NEXT RESEARCH PROGRAM";
        titleTxt.fontSize = 28;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.2f, 1f, 0.2f, 1f); // CRT Green

        // Subtitle
        GameObject subGo = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        subGo.transform.SetParent(panelGo.transform, false);

        RectTransform subRt = subGo.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0.5f, 0.8f);
        subRt.anchorMax = new Vector2(0.5f, 0.8f);
        subRt.pivot = new Vector2(0.5f, 0.5f);
        subRt.anchoredPosition = Vector2.zero;
        subRt.sizeDelta = new Vector2(600f, 40f);

        TextMeshProUGUI subTxt = subGo.GetComponent<TextMeshProUGUI>();
        subTxt.text = "SELECT AN UPGRADE TO UNLOCK IN THE DOCKED STORE";
        subTxt.fontSize = 18;
        subTxt.alignment = TextAlignmentOptions.Center;
        subTxt.color = new Color(0.2f, 0.7f, 0.2f, 0.8f);
    }

    public void OpenUpgradePanel(List<UpgradeDefinition> choices)
    {
        // Clear previous cards
        foreach (var card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();

        // Create cards for each choice
        float cardWidth = 260f;
        float cardSpacing = 300f;
        float startX = -((choices.Count - 1) * cardSpacing) / 2f;

        for (int i = 0; i < choices.Count; i++)
        {
            var choice = choices[i];
            GameObject cardGo = CreateUpgradeCard(choice, new Vector2(startX + (i * cardSpacing), -20f), cardWidth);
            activeCards.Add(cardGo);
        }

        // Display panel
        panelGo.SetActive(true);
        canvasGroup.DOComplete();
        canvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
    }

    private GameObject CreateUpgradeCard(UpgradeDefinition def, Vector2 position, float width)
    {
        GameObject cardGo = new GameObject("UpgradeCard_" + def.upgradeId, typeof(RectTransform), typeof(Image), typeof(Button));
        cardGo.transform.SetParent(panelGo.transform, false);

        RectTransform cardRt = cardGo.GetComponent<RectTransform>();
        cardRt.anchorMin = new Vector2(0.5f, 0.45f);
        cardRt.anchorMax = new Vector2(0.5f, 0.45f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.anchoredPosition = position;
        cardRt.sizeDelta = new Vector2(width, 380f);

        Image cardImg = cardGo.GetComponent<Image>();
        cardImg.color = new Color(0.05f, 0.12f, 0.05f, 0.9f);
        
        Outline outline = cardGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Icon Holder
        GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(cardGo.transform, false);
        RectTransform iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.5f, 0.72f);
        iconRt.anchorMax = new Vector2(0.5f, 0.72f);
        iconRt.pivot = new Vector2(0.5f, 0.5f);
        iconRt.sizeDelta = new Vector2(75f, 75f);
        Image iconImg = iconGo.GetComponent<Image>();
        iconImg.sprite = def.icon;
        iconImg.color = def.icon != null ? Color.white : new Color(0.2f, 0.8f, 0.2f, 0.5f);

        // Title text
        GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(cardGo.transform, false);
        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.45f);
        titleRt.anchorMax = new Vector2(1f, 0.45f);
        titleRt.offsetMin = new Vector2(10f, 0f);
        titleRt.offsetMax = new Vector2(-10f, 50f);
        TextMeshProUGUI titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
        titleTxt.text = def.upgradeName.ToUpper();
        titleTxt.fontSize = 20;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.color = new Color(0.2f, 1f, 0.2f, 1f);

        // Description text
        GameObject descGo = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
        descGo.transform.SetParent(cardGo.transform, false);
        RectTransform descRt = descGo.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0f, 0.15f);
        descRt.anchorMax = new Vector2(1f, 0.15f);
        descRt.offsetMin = new Vector2(12f, 0f);
        descRt.offsetMax = new Vector2(-12f, 110f);
        TextMeshProUGUI descTxt = descGo.GetComponent<TextMeshProUGUI>();
        descTxt.text = def.description;
        descTxt.fontSize = 14;
        descTxt.alignment = TextAlignmentOptions.TopFlush;
        descTxt.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);

        // Selection Action
        Button btn = cardGo.GetComponent<Button>();
        btn.onClick.AddListener(() => {
            cardGo.transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0.05f), 0.2f, 10, 1f).SetUpdate(true);
            
            // Fade out and apply unlock
            canvasGroup.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(() => {
                panelGo.SetActive(false);
                UpgradeManager.Instance.UnlockUpgrade(def);
            });
        });

        // Add Pointer hover animations
        EventTrigger trigger = cardGo.AddComponent<EventTrigger>();
        
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener((data) => {
            cardGo.transform.DOScale(1.05f, 0.15f).SetUpdate(true);
            cardImg.DOColor(new Color(0.1f, 0.25f, 0.1f, 0.95f), 0.15f).SetUpdate(true);
            outline.effectColor = new Color(0.5f, 1f, 0.5f, 0.9f);
        });
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener((data) => {
            cardGo.transform.DOScale(1f, 0.15f).SetUpdate(true);
            cardImg.DOColor(new Color(0.05f, 0.12f, 0.05f, 0.9f), 0.15f).SetUpdate(true);
            outline.effectColor = new Color(0.2f, 0.8f, 0.2f, 0.4f);
        });
        trigger.triggers.Add(exit);

        return cardGo;
    }
}
