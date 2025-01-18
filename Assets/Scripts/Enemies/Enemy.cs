using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MAX_HEALTH;
    public float curr_health;
    public float MOVE_SPEED = 5f;
    public Weapon weapon;

    void MoveAndLook()
    {
        //transform.position = Time.deltaTime;
    }

    void AttackPlayer()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curr_health = MAX_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        if (weapon.range >= Vector3.Distance(this.transform.position, Player.Instance.transform.position)) {
            AttackPlayer();
            // TODO: if in range, attack in current direction
        }
        else
        {
            MoveAndLook();
            // TODO: move towards player and change orientation to face player if out of range
        }

    }
}
