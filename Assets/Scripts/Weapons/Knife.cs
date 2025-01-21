using UnityEngine;

public class Knife : Weapon
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //DEAL DAMAGE
    }
    //NOTE THAT ANY UPDATE, START, OR SIMILARLY NAMED METHODS WILL COMPLETELY OVERRIDE BASE WEAPON CLASS METHODS
}
