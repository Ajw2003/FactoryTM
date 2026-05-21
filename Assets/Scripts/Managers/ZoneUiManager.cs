using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ZoneUiManager : MonoBehaviour
{
    public Button northButton;
    public Button southButton;
    public Button eastButton;
    public Button westButton;

    void Start()
    {
        northButton?.onClick.AddListener(() => ZoneManager.Instance.UnlockNorth());
        southButton?.onClick.AddListener(() => ZoneManager.Instance.UnlockSouth());
        eastButton?.onClick.AddListener(() => ZoneManager.Instance.UnlockEast());
        westButton?.onClick.AddListener(() => ZoneManager.Instance.UnlockWest());
        
        // Removed dynamic text updates as per request
        SetStaticText(northButton, "Unlock North");
        SetStaticText(southButton, "Unlock South");
        SetStaticText(eastButton, "Unlock East");
        SetStaticText(westButton, "Unlock West");
    }

    void SetStaticText(Button button, string text)
    {
        if (button != null)
        {
            var tmp = button.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = text;
        }
    }
}
