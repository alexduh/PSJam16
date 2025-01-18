using UnityEngine;

public class Weapon : MonoBehaviour
{
    protected float FIRE_RATE;
    protected float DAMAGE;
    protected float MAX_AMMO;
    protected float curr_ammo;
    protected bool ranged;
    public float range;

    public void Attack()
    {
        Debug.Log("This weapon is following " + this.name);
        // if weapon is ranged, check if it has ammo remaining
        if (ranged && curr_ammo <= 0)
        {
            JamWeapon();
        }
    }

    public void AttackWindup()
    {

    }

    public void JamWeapon()
    {
        // TODO: play sound effect (animation?)
    }

    public void Throw()
    {
        // TODO: remove Player's currently selected weapon from weapon list
        // TODO: add velocity and collision to thrown weapon
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        curr_ammo = MAX_AMMO;
    }

}
