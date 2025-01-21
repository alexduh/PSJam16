using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum PowerupType { MOVE_SPEED, AMMO_RECHARGE, SLOW_MO, AIM_LOCK };

    PowerupType type;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        type = (PowerupType)Random.Range(0, 3);
    }
}
