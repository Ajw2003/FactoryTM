using Singleton;
using TMPro;
using UnityEngine;

public class UiManager : SingletonBase<UiManager>
{
   public TMP_Text currentCurrency;

   public void UpdateCurrency(int value)
   {
      currentCurrency.text = value.ToString();
   }
}
