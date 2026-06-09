using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponStats", menuName = "Scriptable Objects/WeaponStats")]
public class WeaponStats : ScriptableObject
{
    public float fireRate;
    public float bulletSize;
    public float bulletSpread;
    public float bulletSpeed;
    
    public int bulletDamage;
    public int magazineSize;
    public int bulletsFired;
    public int bulletRange;
    public int burstSize;
    public int reloadSpeed;
    
    public GameObject bulletPrefab;
    
    public AudioClip bulletSound;

    public WeaponType weaponType;

    public int cost;
}

public enum WeaponType
{
    Automatic,
    SemiAutomatic,
    Burst,
    Explosive
}
