using Singleton;
using UnityEngine;

public class CurrencyManager : SingletonBase<CurrencyManager>
{
    public float currentCurrencyValue = 20;

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
        currentCurrencyValue += currencyValue;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
        onCurrencyChange?.Invoke();
    }

    public void RemoveCurrency(float currencyValue)
    {
        currentCurrencyValue -= currencyValue;
        UiManager.Instance.UpdateCurrency(currentCurrencyValue);
        onCurrencyChange?.Invoke();
    }
}
