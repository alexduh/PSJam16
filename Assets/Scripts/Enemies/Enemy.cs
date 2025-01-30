using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MAX_HEALTH;
    public float curr_health;
    public float MOVE_SPEED = 4f;
    private Vector3 m_Velocity = Vector3.zero;
    private float m_MovementSmoothing = .075f;  // How much to smooth out the movement
    [SerializeField] private float rotateSpeed = 60f;

    public Weapon weapon;
    public Rigidbody2D rb;
    public float windUpTime;
    public float cooldownTimeLimit;
    private float cooldownTime;
    [SerializeField] float attackLeeway = 1f; // How much closer enemies will get than their max range
    [SerializeField] Transform gunPoint; // IK grapple target (i.e. the weapon)
    [SerializeField] Transform rig; // IK rig
    [SerializeField] Transform headRig; // IK head
    [SerializeField] LayerMask detectionLayerMask;
    [SerializeField] float detectionRange;
    [SerializeField] GameObject enemyBodySprite;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Enemy colliding!");
    }


    void Start()
    {
        // TODO: assign the weapon before gunPoint is assigned!
        gunPoint = weapon.transform;
        rb = GetComponent<Rigidbody2D>();
        weapon.movementSpeed = MOVE_SPEED;
        curr_health = MAX_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        Look();

        if (cooldownTime < cooldownTimeLimit)
        {
            weapon.AttackWindup(false);
            cooldownTime += Time.deltaTime;
            return;
        }

        Debug.DrawLine(transform.position, transform.position + Vector3.up * detectionRange, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Player.Instance.transform.position - transform.position, detectionRange, detectionLayerMask);
        if (hit.transform == null) return;
        if (!hit.transform.CompareTag("Player"))
        {
            weapon.AttackWindup(false);
            cooldownTime = cooldownTimeLimit;
            return;
        }

        if (weapon.range >= Vector3.Distance(Player.Instance.transform.position, gunPoint.position))
        {
            weapon.AttackWindup(true);
            if (cooldownTime > (cooldownTimeLimit + windUpTime))
            {
                AttackPlayer();
                cooldownTime = 0;
            }
            else
            {
                cooldownTime += Time.deltaTime;
            }
        }
        else 
        {
            Move();
            cooldownTime = cooldownTimeLimit;
            weapon.AttackWindup(false); 
        }
    }

    void Move(float magnitude = 1f) // move towards player
    {
        Vector3 direction = Player.Instance.transform.position - gunPoint.position * magnitude;

        float angle = Mathf.Atan(direction.y / direction.x) * Mathf.Rad2Deg;
        if (direction.y < 0) angle -= 180;
        else angle += 180;
        if (direction.x < 0) angle -= 90;
        else angle += 90; // help me

        var destination = moveTowardsPerimeter(Player.Instance.transform.position, weapon.range - attackLeeway, angle);
        weapon.setDestination = destination;

        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, (destination - transform.position) * MOVE_SPEED, ref m_Velocity, m_MovementSmoothing);

    }

    // move the enemy towards whatever point is closest on an circular perimeter around the player
    Vector3 moveTowardsPerimeter(Vector3 center, float radius, float angle)
    {
        float angleInRadians = angle * Mathf.Deg2Rad;
        float x = center.x + radius * Mathf.Cos(angleInRadians);
        float y = center.y + radius * Mathf.Sin(angleInRadians);
        return new Vector3(x, y, center.z);
    }

    void Look() // change orientation to face player
    {
        Vector3 direction = (Player.Instance.transform.position - gunPoint.position).normalized;

        // Calculate the angle between the player's forward direction and the direction to the mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Smoothly rotate the player towards the mouse position
        float step = rotateSpeed * Time.deltaTime;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));

        /*
        float angle = Mathf.Atan(direction.y /direction.x) * Mathf.Rad2Deg;
        if (direction.y < 0) angle -= 180;
        else angle += 180;
        if (direction.x < 0) angle -= 90;
        else angle += 90; // help me

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        */

        
        gunPoint.rotation = Quaternion.RotateTowards(gunPoint.rotation, targetRotation, step);
        weapon.transform.rotation = Quaternion.RotateTowards(weapon.transform.rotation, targetRotation, step);

    }

    public void AttackPlayer()
    {
        weapon.AttackWindup(false);
        weapon.Attack(false);
    }

    void SetHP(float hp)
    {
        curr_health = hp;
    }

    void Death()
    {
        // TODO: add sounds and animation
        weapon.Throw();
        enemyBodySprite.transform.parent = null;
        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        SetHP(curr_health - damage);
        if (curr_health <= 0)
            Death();
    }

}
