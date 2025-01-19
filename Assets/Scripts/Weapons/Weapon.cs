using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    [SerializeField] protected float FIRE_RATE;
    protected float attackCooldownTime;
    [SerializeField] protected float DAMAGE;
    [SerializeField] protected float weight;
    [SerializeField] protected Rigidbody2D rb;


    //Variables related to visuals
    [SerializeField] protected GameObject heldSprite;
    [SerializeField] protected GameObject droppedSprite;
    [SerializeField] protected float movementSpeed = 1f;
    public Vector2 setDestination;
    private Vector3 m_Velocity = Vector3.zero;
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .075f;  // How much to smooth out the movement

    //Variables Related to ranged projectiles
    [SerializeField] protected bool ranged;
    public float range;
    [SerializeField] protected float MAX_AMMO;
    [SerializeField] protected float curr_ammo;
    [SerializeField] GameObject projectile;
    [SerializeField] Transform weaponSpawnPoint;
    [SerializeField] protected float projSpeed;
    [SerializeField] protected float projLifeTime;
    [SerializeField] protected float projSpread;
    [SerializeField] protected int numProj;
    [SerializeField] protected float timeBetweenProj;
    public bool friendlyFire; // used to determine if attacks should collide with enemies or player

    protected void Start()
    {
        curr_ammo = MAX_AMMO;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //Controls attack Cooldown time. If attackCooldownTime <= 0, then this weapon can attack.
        if(attackCooldownTime >= 0) attackCooldownTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        MoveToTarget();
    }
    public void Attack(bool friendly)
    {
        if (attackCooldownTime > 0) return;
        attackCooldownTime = FIRE_RATE;
        Debug.Log("This weapon is attacking: " + this.name);

        if (ranged)
        {
            RangedAttack();
        }
        else
        {
            MeleeAttack();
        }
    }

    //Method Holder for Ranged Attacks. This is to keep basic functionality (Attack cooldowns) standard, but seperate ranged vs melee
    //This allows children classes to modifiy ranged behavior without messing with basic attack behavior
    protected void RangedAttack()
    {
        if (curr_ammo <= 0) { JamWeapon(); return; }
        StartCoroutine(ShootProjectiles(timeBetweenProj, numProj));
    }

    //Method Holder for Melee Attacks. This is to keep basic functionality (Attack cooldowns) standard, but seperate ranged vs melee
    //This allows children classes to modifiy melee behavior without messing with basic attack behavior
    protected void MeleeAttack()
    {
        //TODO: Melee Attack
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
        ToggleSprite(false);
        // TODO: add velocity and collision to thrown weapon
    }

    public void Pickup()
    {
        ToggleSprite(true);
    }

    protected void ToggleSprite(bool held)
    {
        heldSprite.SetActive(held);
        droppedSprite.SetActive(!held);
    }

    //Coroutine that shoots multiple projectiles in a row
    protected IEnumerator ShootProjectiles(float timeBetweenProj, int numProj)
    {
        for (int i = 0; i < numProj; i++)
        {
            ShootProjectile();
            yield return new WaitForSeconds(timeBetweenProj);
        }
        
    }

    //Shoots projectile based ranged attack
    protected void ShootProjectile()
    {
        //Creates Offset and projectile direction
        Vector2 offset = new Vector2(Random.Range(-projSpread, projSpread), Random.Range(-projSpread, projSpread));
        Vector2 direction = this.transform.up.normalized;
        direction = (direction + offset).normalized * projSpeed;
        ObjectPool.Instance.GetPooledObject(projectile).GetComponent<Projectile>().ActivateProjectile(weaponSpawnPoint.position, direction, DAMAGE, projLifeTime);
        curr_ammo -= 1;
    }

    private void MoveToTarget()
    {
        Vector2 unNormalized = setDestination - new Vector2(transform.position.x, transform.position.y);
        Vector2 directionVector = unNormalized;
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, directionVector * movementSpeed, ref m_Velocity, m_MovementSmoothing);
    }

}
