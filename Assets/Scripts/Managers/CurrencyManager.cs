using Managers;
using Placeables;
using Ui;
using Weapons;
using Nodes;
using EventTypes;
using EventTypes.InventoryEvents;
using EventTypes.InputEvents;
namespace Managers
{
    using Singleton;
    using UnityEngine;
    
    public class CurrencyManager : SingletonBase<CurrencyManager>
    {
        public float currentCurrencyValue = 20;
        public float incomeMultiplier = 1.0f;
    
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
            float valueToAdd = currencyValue * incomeMultiplier;
            currentCurrencyValue += valueToAdd;
            
            if (Managers.GameStatsManager.HasInstance)
            {
                Managers.GameStatsManager.Instance.AddMoneyEarned(valueToAdd);
            }
    
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
    
}


