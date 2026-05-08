using Singleton;
using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    public int currentCurrencyValue;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCurrency(int currencyValue)
    {
        currentCurrencyValue += currencyValue;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
    }

    public void RemoveCurrency(int currencyValue)
    {
        currentCurrencyValue -= currencyValue;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
    }
}
