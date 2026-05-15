using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZoneUiManager : MonoBehaviour
{
    public Button northButton;
    public Button southButton;
    public Button eastButton;
    public Button westButton;

    private TMP_Text northText;
    private TMP_Text southText;
    private TMP_Text eastText;
    private TMP_Text westText;

    void Start()
    {
        if (northButton != null) northText = northButton.GetComponentInChildren<TMP_Text>();
        if (southButton != null) southText = southButton.GetComponentInChildren<TMP_Text>();
        if (eastButton != null) eastText = eastButton.GetComponentInChildren<TMP_Text>();
        if (westButton != null) westText = westButton.GetComponentInChildren<TMP_Text>();

        northButton?.onClick.AddListener(() => ZoneManager.Instance.Navigate(Vector2Int.up));
        southButton?.onClick.AddListener(() => ZoneManager.Instance.Navigate(Vector2Int.down));
        eastButton?.onClick.AddListener(() => ZoneManager.Instance.Navigate(Vector2Int.right));
        westButton?.onClick.AddListener(() => ZoneManager.Instance.Navigate(Vector2Int.left));
        
        northButton?.onClick.AddListener(UpdateButtons);
        southButton?.onClick.AddListener(UpdateButtons);
        eastButton?.onClick.AddListener(UpdateButtons);
        westButton?.onClick.AddListener(UpdateButtons);

        UpdateButtons();

    }

    void UpdateButtons()
    {
        if (ZoneManager.Instance == null) return;

        Vector2Int current = ZoneManager.Instance.GetCurrentZone();
        float cost = ZoneManager.Instance.GetUnlockCost() * CurrencyManager.Instance.exchangeRate;

        UpdateButton(northText, current + Vector2Int.up, "North", cost);
        UpdateButton(southText, current + Vector2Int.down, "South", cost);
        UpdateButton(eastText, current + Vector2Int.right, "East", cost);
        UpdateButton(westText, current + Vector2Int.left, "West", cost);
    }

    void UpdateButton(TMP_Text text, Vector2Int targetZone, string directionName, float cost)
    {
        if (text == null) return;

        if (ZoneManager.Instance.IsZoneUnlocked(targetZone))
        {
            text.text = "Go " + directionName;
        }
        else
        {
            text.text = "Unlock " + directionName + "\n($" + cost + ")";
        }
    }
}
