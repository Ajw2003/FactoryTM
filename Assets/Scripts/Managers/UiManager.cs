using Singleton;
using TMPro;
using UnityEngine;

public class UiManager : SingletonBase<UiManager>
{
   public TMP_Text currentCurrency;
   
   [Header("Tooltip")]
   public GameObject tooltipPanel;
   public TMP_Text tooltipText;
   public GameObject StorePanel;
   private RectTransform tooltipRect;
   bool flip = true;
   

   protected override void Awake()
   {
      base.Awake();
      if (tooltipPanel != null)
      {
         tooltipRect = tooltipPanel.GetComponent<RectTransform>();
         tooltipPanel.SetActive(false);
      }
   }

   private void Update()
   {
      if (tooltipPanel != null && tooltipPanel.activeSelf)
      {
         UpdateTooltipPosition();
      }
      
      if (Input.GetKeyDown(KeyCode.E))
      {
         
         flip = !flip;
         StorePanel.SetActive(flip);
      }
   }

   private void UpdateTooltipPosition()
   {
      Vector2 mousePos = Input.mousePosition;
      // Offset to avoid being directly under the cursor (prevents flickering)
      tooltipRect.position = mousePos + new Vector2(15, -15);
   }

   public void UpdateCurrency(float value)
   {
      currentCurrency.text = value.ToString();
   }

   public void ShowTooltip(string description)
   {
      if (tooltipPanel != null && tooltipText != null)
      {
         tooltipText.text = description;
         tooltipPanel.SetActive(true);
         UpdateTooltipPosition(); // Position it immediately
      }
   }

   public void HideTooltip()
   {
      if (tooltipPanel != null)
      {
         tooltipPanel.SetActive(false);
      }
   }
}
