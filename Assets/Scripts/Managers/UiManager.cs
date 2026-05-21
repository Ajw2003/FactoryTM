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
   public GameObject HotbarPanel; // New reference
   private RectTransform tooltipRect;
   bool flip = false; // Start closed

   protected override void Awake()
   {
      base.Awake();
      if (tooltipPanel != null)
      {
         tooltipRect = tooltipPanel.GetComponent<RectTransform>();
         tooltipPanel.SetActive(false);
      }
      
      // Ensure initial state
      if (StorePanel != null) StorePanel.SetActive(flip);
      if (HotbarPanel != null) HotbarPanel.SetActive(!flip);
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
         if (StorePanel != null) StorePanel.SetActive(flip);
         if (HotbarPanel != null) HotbarPanel.SetActive(!flip); // Close Hotbar when Store is open
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
