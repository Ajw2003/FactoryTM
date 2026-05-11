using Singleton;
using TMPro;
using UnityEngine;

public class UiManager : SingletonBase<UiManager>
{
   public TMP_Text currentCurrency;

   public void UpdateCurrency(float value)
   {
      currentCurrency.text = value.ToString();
   }
}
