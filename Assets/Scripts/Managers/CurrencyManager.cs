using Singleton;
using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    public float currentCurrencyValue = 40000;
    public float exchangeRate = 200;

    public delegate void OnCurrencyChange();
    public event OnCurrencyChange onCurrencyChange;

    protected override void Awake()
    {
        persistBetweenScenes = false;
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
        onCurrencyChange?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCurrency(float currencyValue)
    {
        currentCurrencyValue += currencyValue * exchangeRate;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
        onCurrencyChange?.Invoke();
    }

    public void RemoveCurrency(float currencyValue)
    {
        currentCurrencyValue -= currencyValue * exchangeRate;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
        onCurrencyChange?.Invoke();
    }
}
