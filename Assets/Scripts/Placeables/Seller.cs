using System;
using UnityEngine;

public class Seller : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out ConveyorItem item))
        {
            CurrencyManager.Instance.AddCurrency(item.value);
        }
    }
}
