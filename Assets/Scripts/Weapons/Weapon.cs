using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    [SerializeField] protected float FIRE_RATE;
    protected float attackCooldownTime;
    [SerializeField] protected float DAMAGE;
    public float weight;
    protected Rigidbody2D rb;
    


    //Variables related to visuals
    [SerializeField] protected GameObject heldSprite;
    [SerializeField] protected GameObject droppedSprite;
    [SerializeField] float playerMovementSpeed = 4f;
    public float movementSpeed = 4f;
    private float FADE_TIME = 3f;
    public Vector2 setDestination;
    private Vector3 m_Velocity = Vector3.zero;
    private float m_MovementSmoothing = .075f;  // How much to smooth out the movement

    //Variables Related to ranged projectiles
    [SerializeField] protected bool ranged;
    [SerializeField] protected float MAX_AMMO;


    public float curr_ammo; //Made public so player can detect if the weapon is empty
    [SerializeField] GameObject projectile;
    [SerializeField] Transform weaponSpawnPoint;
    [SerializeField] protected float projSpeed;
    [SerializeField] protected float projLifeTime;
    [SerializeField] protected float projSpread;
    [SerializeField] protected int numProj;
    [SerializeField] protected float timeBetweenProj;
    [SerializeField] Collider2D weaponCollider;
    [SerializeField] Transform projectileHelper;

    public bool friendlyFire; // used to determine if attacks should collide with enemies or player
    [SerializeField] protected LineRendererHelper enemyAimingLineRenderer;
    [SerializeField] LayerMask rangedLayerMask;


    [SerializeField] AudioSource weaponFireController;
    [SerializeField] AudioClip[] weaponFireSounds;



    protected void Start()
    {
        setDestination = transform.position;
        curr_ammo = MAX_AMMO;
        rb = GetComponent<Rigidbody2D>();
    }

    protected void Update()
    {
        //Controls attack Cooldown time. If attackCooldownTime <= 0, then this weapon can attack.
        if(attackCooldownTime >= 0) attackCooldownTime -= Time.deltaTime;
    }

    protected void FixedUpdate()
    {
        MoveToTarget();
    }

    //Attack method for both enemies and the player, differentinated by the friendly bool. Checks for cool down time, and seperates
    //Attacks based on range or melee
    public void Attack(bool friendly)
    {
        if (attackCooldownTime > 0) return;
        attackCooldownTime = FIRE_RATE;
        Debug.Log("This weapon is attacking: " + this.name);

        if (ranged)
        {
            RangedAttack(friendly);
        }
        else
        {
            MeleeAttack();
        }
    }

    //Method Holder for Ranged Attacks. This is to keep basic functionality (Attack cooldowns) standard, but seperate ranged vs melee
    //This allows children classes to modifiy ranged behavior without messing with basic attack behavior
    protected void RangedAttack(bool friendly)
    {
        StartCoroutine(ShootProjectiles(timeBetweenProj, numProj, friendly));
    }

    //Method Holder for Melee Attacks. This is to keep basic functionality (Attack cooldowns) standard, but seperate ranged vs melee
    //This allows children classes to modifiy melee behavior without messing with basic attack behavior
    protected void MeleeAttack()
    {
        //TODO: Melee Attack
    }

    //Called by enemies when an attack is about to be executed.
    public void AttackWindup(bool activate = true)
    {
        if (!activate) { enemyAimingLineRenderer.EraseLines(); return; }

        RaycastHit2D hit = Physics2D.Raycast(weaponSpawnPoint.position, transform.up, projSpeed * projLifeTime, rangedLayerMask);
        if (hit)
        {
            projectileHelper.transform.position = hit.point;
            Transform[] points = { weaponSpawnPoint, projectileHelper };
            enemyAimingLineRenderer.SetUpLine(points);
        }
        else
        {
            projectileHelper.transform.position = projectileHelper.transform.position + (transform.up * projSpeed * projLifeTime);
            Transform[] points = { weaponSpawnPoint, projectileHelper };
            enemyAimingLineRenderer.SetUpLine(points);
        }
    }

    //Called when ammo runs out
    public void JamWeapon()
    {
        Debug.Log("Weapon Jammed");

        //If the weapon is attatched to a player, then the weapon will be thrown by the player. There is extra code to remove
        //The weapon from the weapon list, along with a back reference to the throw method in this script
        if (friendlyFire)
        {
            FindAnyObjectByType<Player>().ThrowWeapon(this);
        }
        // TODO: play sound effect (animation?)
    }

    public void Drop()
    {
        ToggleSprite(false);
        StartCoroutine(FadeAway());
    }

    //Behavior for when the player droppes/throws a weapon
    public void Throw()
    {
        transform.SetParent(null);
        ToggleSprite(false);
        enemyAimingLineRenderer.EraseLines();
        // add velocity and collision to thrown weapon
        weaponCollider.enabled = true;
        rb.linearVelocity = this.transform.up.normalized * projSpeed;
        tag = "PlayerOwned"; // <-- this is a stopgap to allow thrown weapons to hurt enemy, however they should lose the tag once the throw is 'over'.
        if(ranged && curr_ammo == 0) StartCoroutine(FadeAway());
        // TODO: add velocity and collision to thrown weapon
    }


    //Weapon side behavior for behing picked up by the player. Toggles to the held sprite and sets friendly fire to true;
    public void Pickup()
    {
        ToggleSprite(true);
        TogglePickUpAble(false);
        movementSpeed = playerMovementSpeed;
        friendlyFire = true;
        weaponCollider.enabled = false;
    }

    //Toggles the sprite to be highlighted yellow. Called when the player is near the sprite
    public void TogglePickUpAble(bool isPickupAble)
    {
        if(isPickupAble)
        {
            droppedSprite.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else
        {
            droppedSprite.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    //Toggles the sprite between active and held sprites. Placeholder for animation
    protected void ToggleSprite(bool held)
    {
        heldSprite.SetActive(held);
        droppedSprite.SetActive(!held);
    }

    protected IEnumerator FadeAway()
    {
        Color currentColor = droppedSprite.GetComponent<SpriteRenderer>().color;
        Color targetColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0);
        float currentTime = Time.time;
        while (droppedSprite.GetComponent<SpriteRenderer>().color != targetColor)
        {
            droppedSprite.GetComponent<SpriteRenderer>().color = Color.Lerp(currentColor, targetColor, (Time.time - currentTime)/FADE_TIME);
            yield return new WaitForSeconds(.1f);
        }

        yield return null;
    }

    //Coroutine that shoots multiple projectiles in a row
    protected IEnumerator ShootProjectiles(float timeBetweenProj, int numProj, bool friendly)
    {
        for (int i = 0; i < numProj; i++)
        {
            ShootProjectile(friendly);
            yield return new WaitForSeconds(timeBetweenProj);
        }
        
    }

    //Shoots projectile based ranged attack
    protected void ShootProjectile(bool friendly)
    {

        //Creates Offset and projectile direction
        if (curr_ammo <= 0) { JamWeapon(); return; }
        Vector2 offset = new Vector2(Random.Range(-projSpread, projSpread), Random.Range(-projSpread, projSpread));
        Vector2 direction = this.transform.up.normalized;
        direction = (direction + offset).normalized * projSpeed;
        ObjectPool.Instance.GetPooledObject(projectile).GetComponent<Projectile>().ActivateProjectile(weaponSpawnPoint.position, direction, DAMAGE, projLifeTime, friendlyFire);
        if (friendlyFire)// enemies will have infinite ammo
        {
            curr_ammo -= 1;
            if (curr_ammo <= 0) FindAnyObjectByType<Player>().ThrowWeapon(this);
        }

        if (weaponFireSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, weaponFireSounds.Length - 1);
            AudioClip selectedSound = weaponFireSounds[randomIndex];

            weaponFireController.PlayOneShot(selectedSound);
        }
    }

    //Helper class that smoothly moves the weapon to its target position
    private void MoveToTarget()
    {
        Vector2 unNormalized = setDestination - new Vector2(transform.position.x, transform.position.y);
        Vector2 directionVector = unNormalized;
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, directionVector * movementSpeed, ref m_Velocity, m_MovementSmoothing);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!ranged)
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(DAMAGE);
        }
    }

}
