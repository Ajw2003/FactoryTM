using Singleton;
using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    public float currentCurrencyValue;
    public float exchangeRate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCurrency(float currencyValue)
    {
        currentCurrencyValue += currencyValue * exchangeRate;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
    }

    public void RemoveCurrency(float currencyValue)
    {
        currentCurrencyValue -= currencyValue * exchangeRate;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
    }
}
