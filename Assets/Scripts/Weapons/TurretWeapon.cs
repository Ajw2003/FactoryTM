using UnityEngine;

public class TurretWeapon : BaseWeapon
{
    [SerializeField] private float rotationSpeed = 5f;

    public bool hasTarget = false;

    protected override void Update()
    {
        base.Update();
        
        if (hasTarget && canFire && roundsLeft > 0)
        {
            // Rotate towards target
            Vector2 direction = target - (Vector2)transform.position;
            if (direction.sqrMagnitude > 0.01f)
            {
                float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90f;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
            }

            Shoot();
        }
    }

    public void SetupWeapon(GameObject projectilePrefab, float defaultFireRate = 1f, int defaultDamage = 10, float defaultSpeed = 10f, int defaultBulletsFired = 1, float defaultSpread = 0f, WeaponType defaultWeaponType = WeaponType.Automatic)
    {
        bulletPrefab = projectilePrefab;
        fireRate = defaultFireRate;
        bulletDamage = defaultDamage;
        bulletSpeed = defaultSpeed;
        magazineSize = 99999;
        roundsLeft = magazineSize;
        bulletsFired = defaultBulletsFired;
        bulletSpread = defaultSpread;
        weaponType = defaultWeaponType;
        nextTimeToFire = Time.time + fireRate;
    }

    // Apply tier multipliers to weapon stats
    public void ApplyTierMultiplier(float multiplier)
    {
        // Adjust current stats based on multiplier directly
        fireRate /= multiplier; // Fire faster
        bulletDamage = Mathf.RoundToInt(bulletDamage * multiplier); // Do more damage
        nextTimeToFire = Time.time + fireRate;
    }
}
