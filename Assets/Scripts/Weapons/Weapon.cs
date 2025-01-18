using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] protected float FIRE_RATE;
    protected float attackCooldownTime;
    [SerializeField] protected float DAMAGE;
    [SerializeField] protected float weight;

    //Variables Related to ranged projectiles
    [SerializeField] protected bool ranged;
    public float range;
    [SerializeField] protected float MAX_AMMO;
    [SerializeField] protected float curr_ammo;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform weaponSpawnPoint;
    [SerializeField] protected float projSpeed;
    [SerializeField] protected float projLifeTime;

    protected void Start()
    {
        curr_ammo = MAX_AMMO;
    }

    private void Update()
    {

    }
    public void Attack()
    {
        Debug.Log("This weapon is attacking: " + this.name);
        // if weapon is ranged, check if it has ammo remaining
        if (ranged)
        {
            if (curr_ammo <= 0) { JamWeapon(); }
            ObjectPool.Instance.GetPooledObject(projectile).GetComponent<Projectile>().ActivateProjectile(weaponSpawnPoint.position, this.transform.up.normalized * projSpeed, DAMAGE, projLifeTime);
        }
    }

    public void AttackWindup()
    {

    }

    public void JamWeapon()
    {
        Debug.Log("Weapon Jammed");
        // TODO: play sound effect (animation?)
    }

    public void Throw()
    {
        // TODO: remove Player's currently selected weapon from weapon list
        // TODO: add velocity and collision to thrown weapon
    }

    

}
