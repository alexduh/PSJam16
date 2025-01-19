using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MAX_HEALTH;
    public float curr_health;
    public float MOVE_SPEED = 5f;
    public Weapon weapon;
    public Rigidbody2D rb;
    public float windUpTime;
    public float cooldownTime;

    void MoveAndLook() // move towards player and change orientation to face player if out of range
    {
        rb.linearVelocity = (this.transform.position - Player.Instance.transform.position).normalized;
        transform.LookAt(Player.Instance.transform);
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        curr_health = MAX_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        if (curr_health <= 0)
        {
            Death();
        }

        if (windUpTime > 0 || cooldownTime > 0)
        {

        }
        if (weapon.range >= Vector3.Distance(this.transform.position, Player.Instance.transform.position)) {
            AttackPlayer();
            // TODO: if in range, attack in current direction
        }
        else
        {
            MoveAndLook();
        }

    }
}
