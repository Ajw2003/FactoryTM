using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private GameObject reloadingText;

    public void UpdateAmmo(int current, int max)
    {
        if (ammoText != null)
        {
            ammoText.text = $"{current} / {max}";
        }
    }

    public void SetReloading(bool isReloading)
    {
        if (reloadingText != null)
        {
            reloadingText.SetActive(isReloading);
        }
    }
}
