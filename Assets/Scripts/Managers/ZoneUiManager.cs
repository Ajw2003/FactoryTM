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
        if (northButton == null) northButton = transform.Find("NorthButton")?.GetComponent<Button>();
        if (southButton == null) southButton = transform.Find("SouthButton")?.GetComponent<Button>();
        if (eastButton == null) eastButton = transform.Find("EastButton")?.GetComponent<Button>();
        if (westButton == null) westButton = transform.Find("WestButton")?.GetComponent<Button>();

        // Attach dynamic feedback and click logic to the zone buttons
        SetupZoneButton(northButton, UiZoneButton.Direction.North);
        SetupZoneButton(southButton, UiZoneButton.Direction.South);
        SetupZoneButton(eastButton, UiZoneButton.Direction.East);
        SetupZoneButton(westButton, UiZoneButton.Direction.West);
        
        // Removed dynamic text updates as per request
        SetStaticText(northButton, "Unlock North");
        SetStaticText(southButton, "Unlock South");
        SetStaticText(eastButton, "Unlock East");
        SetStaticText(westButton, "Unlock West");
    }

    void SetupZoneButton(Button button, UiZoneButton.Direction dir)
    {
        if (button != null)
        {
            UiZoneButton zb = button.gameObject.AddComponent<UiZoneButton>();
            zb.direction = dir;
        }
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
