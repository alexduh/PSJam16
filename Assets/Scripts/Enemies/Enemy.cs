using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MAX_HEALTH;
    public float curr_health;
    public float MOVE_SPEED = 4f;
    private Vector3 m_Velocity = Vector3.zero;
    private float m_MovementSmoothing = .075f;  // How much to smooth out the movement

    public Weapon weapon;
    public Rigidbody2D rb;
    public float windUpTime;
    public float cooldownTime;
    [SerializeField] float attackLeeway = 1f; // How much closer enemies will get than their max range
    [SerializeField] Transform gunPoint; // IK grapple target (i.e. the weapon)
    [SerializeField] Transform rig; // IK rig
    [SerializeField] Transform headRig; // IK head


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        weapon.movementSpeed = MOVE_SPEED;
        curr_health = MAX_HEALTH;
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
        Vector3 direction = Player.Instance.transform.position - gunPoint.position;

        float angle = Mathf.Atan(direction.y /direction.x) * Mathf.Rad2Deg;
        if (direction.y < 0) angle -= 180;
        else angle += 180;
        if (direction.x < 0) angle -= 90;
        else angle += 90; // help me

        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        gunPoint.rotation = targetRotation;
        weapon.transform.rotation = targetRotation;

    }

    void AttackPlayer()
    {
        weapon.AttackWindup();
        weapon.Attack(false);
    }

    void SetHP(float hp)
    {
        curr_health = hp;
    }

    void Death()
    {
        // TODO: add sounds and animation
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        SetHP(curr_health - damage);
        if (curr_health <= 0)
            Death();
    }

    // Update is called once per frame
    void Update()
    {
        Look();

        if (windUpTime > 0 || cooldownTime > 0)
        {
            return;
        }

        if (weapon.range >= Vector3.Distance(Player.Instance.transform.position, gunPoint.position))
        {
            AttackPlayer();

            // TODO: if in range, attack in current direction
        }
        else Move();
    }
}
