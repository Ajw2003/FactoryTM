using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
namespace Ui
{
    using TMPro;
    using UnityEngine;
    
    public class AmmoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private GameObject reloadingText;
    
        private void Awake()
        {
            if (ammoText == null)
            {
                Transform t = transform.Find("AmmoText");
                if (t == null) t = transform.Find("Text");
                if (t != null) ammoText = t.GetComponent<TMP_Text>();
            }
    
            if (reloadingText == null)
            {
                Transform t = transform.Find("ReloadingText");
                if (t != null) reloadingText = t.gameObject;
            }
        }
    
        public void UpdateAmmo(int current, int max, int reserve)
        {
            if (ammoText != null)
            {
                ammoText.text = $"{current} / {max} [{reserve}]";
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
    
}


