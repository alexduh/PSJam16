using UnityEngine;

public class Pistol : Weapon
{
    new void Start()
    {
        /*
        base.Start();
        MAX_AMMO = 12;
        projSpeed = 10;
        projLifeTime = .5f;
        projSpread = 0;
        numProj = 1;
        timeBetweenProj = .5f;
        range = projSpeed * projLifeTime;
        */
    }

    new void Update()
    {
        
    }

    //NOTE THAT ANY UPDATE, START, OR SIMILARLY NAMED METHODS WILL COMPLETELY OVERRIDE BASE WEAPON CLASS METHODS
}
