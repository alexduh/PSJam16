using System.Collections;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    enum Behavior
    {
        Passive,
        Chase,
        Attack,
    }

    public float MAX_HEALTH;
    public float curr_health;
    public float MOVE_SPEED = 4f;
    private Vector3 m_Velocity = Vector3.zero;
    private float m_MovementSmoothing = .075f;  // How much to smooth out the movement
    [SerializeField] private float rotateSpeed = 60f;

    [SerializeField] private Behavior enemyBehavior;

    public Weapon weapon;
    public Rigidbody2D rb;
    public float windUpTime;
    public float cooldownTimeLimit;
    private float cooldownTime;
    [SerializeField] float followDistance = 8f; // How much closer enemies will get than their max range
    [SerializeField] Transform gunPoint; // IK grapple target (i.e. the weapon)
    [SerializeField] Transform rig; // IK rig
    [SerializeField] Transform headRig; // IK head
    [SerializeField] LayerMask detectionLayerMask;
    [SerializeField] float detectionRange;
    [SerializeField] GameObject enemyBodySprite;

    //For Pathfinding
    private GameObject player;
    private AIPath path;
    private float distanceToTarget;
    public bool isAggressive = false;
    Vector3 startingPos;

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
        player = Player.Instance.gameObject;
        path = GetComponent<AIPath>();
        startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (cooldownTime < cooldownTimeLimit)
        {
            weapon.AttackWindup(false);
            cooldownTime += Time.deltaTime;
        }

        distanceToTarget = Vector3.Distance(player.transform.position, gunPoint.position);
        Debug.DrawLine(transform.position, transform.position + Vector3.up * detectionRange, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, player.transform.position - transform.position, detectionRange, detectionLayerMask);

        switch (enemyBehavior)
        {
            default:
            case Behavior.Passive:
                weapon.AttackWindup(false);
                if (isAggressive)
                {
                    Look();
                    Move(player.transform.position);
                }
                else
                {
                    Move(startingPos);
                }

                if (hit.transform != null)
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        enemyBehavior = Behavior.Chase;
                    }
                }
                break;

            case Behavior.Chase:
                weapon.AttackWindup(false);
                Look();
                Move(player.transform.position, followDistance);
                if (distanceToTarget < detectionRange * 1.25f)
                {
                    if (cooldownTime >= cooldownTimeLimit) enemyBehavior = Behavior.Attack;
                }


                if (hit.transform == null) ;
                else if (!hit.transform.CompareTag("Player"))
                {
                    enemyBehavior = Behavior.Passive;
                }

                break;
            case Behavior.Attack:
                Look();
                path.maxSpeed = 0;
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

                if (hit.transform == null)
                {
                    if (distanceToTarget < detectionRange * 1.25f)
                    {
                        enemyBehavior = Behavior.Chase;
                    }
                }
                else if (!hit.transform.CompareTag("Player"))
                {
                    cooldownTime = cooldownTimeLimit;
                    enemyBehavior = Behavior.Chase;
                }
                break;
        }

        
    }

    void Move(Vector3 targetPos, float actualFollowDistance = 0)
    {
        path.maxSpeed = MOVE_SPEED;
        path.destination = targetPos;
        if (path.remainingDistance < actualFollowDistance)
        {
            path.destination = transform.position;
        }
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, (path.destination - targetPos) * MOVE_SPEED, ref m_Velocity, m_MovementSmoothing);
        weapon.setDestination = transform.position;
    }

    // DEPRECIATED FOR PATHFINDING
    /*

    void Move(float magnitude = 1f) // move towards player
    {
        Vector3 direction = Player.Instance.transform.position - gunPoint.position * magnitude;

        float angle = Mathf.Atan(direction.y / direction.x) * Mathf.Rad2Deg;
        if (direction.y < 0) angle -= 180;
        else angle += 180;
        if (direction.x < 0) angle -= 90;
        else angle += 90; // help me

        var destination = moveTowardsPerimeter(player.transform.position, weapon.range - attackLeeway, angle);
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

    */

    void Look() // change orientation to face player
    {
        Vector3 direction = (player.transform.position - gunPoint.position).normalized;

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
        enemyBehavior = Behavior.Passive;
    }

    void SetHP(float hp)
    {
        curr_health = hp;
    }

    void Death()
    {
        // TODO: add sounds and animation
        startingPos = transform.position;
        weapon.setDestination = transform.position;
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
