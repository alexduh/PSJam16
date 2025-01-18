using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float MAX_HEALTH;
    public float curr_health;
    public float MOVE_SPEED;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        curr_health = MAX_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: move towards player and change orientation to face player if out of range

        // TODO: if in range, attack in current direction
    }
}
