using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

public class Player : Singleton<Player>
{

    [SerializeField] float maxMoveSpeed = 60f;
    [SerializeField] float minMoveSpeed = 5f;
    [SerializeField] float moveForce = 400f;
    [SerializeField] float changeVelocitySpeed;
    float actualSpeed;
    private float orbitOffset = 0f; // cycles upwards from 0 to 360; allows weapon cloud objects to orbit
    [SerializeField] private float orbitSpeed = 60f;
    [SerializeField] float rotateSpeed = 10f;
    Vector2 moveVector;
    Vector2 cursorPos;
    [SerializeField] PlayerInput playerInput;
    InputAction moveAction; InputAction attackAction; InputAction attackAllAction; InputAction interactAction; InputAction throwAction; InputAction previousWeaponAction; InputAction nextWeaponAction;
    Rigidbody2D rb;
    [SerializeField] CircleCollider2D playerCollider;

    [SerializeField] Knife swordPrefab;
    [SerializeField] Transform weaponHolder; // picked up weapons will be parented to this object

    [SerializeField] List<Weapon> weaponList = new List<Weapon>(); //This is the core of the class. Contains a list of all weapons avaliable to the player
    [SerializeField] int weaponIndex = 0; //Index of the active weapon. Change this to change teh active weapon.
    [SerializeField] float weaponRevolveRadius;
    [SerializeField] List<Weapon> sameWeaponTypeList = new List<Weapon>(); //List of weapons that are the same as the active weapon. Recalcalculated whenever the active weapon changes
    [SerializeField] List<string> uniqueWeaponList = new List<string>();


    List<Weapon> nearbyWeaponsList = new List<Weapon>(); //List of weapons that are nearby
    

    [SerializeField] float pickupRange;
    [SerializeField] LayerMask weaponDetectionLayers;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = playerInput.actions["Move"];
        attackAction = playerInput.actions["Attack"];
        attackAllAction = playerInput.actions["AttackAll"];
        throwAction = playerInput.actions["Throw"];
        interactAction = playerInput.actions["Interact"];
        previousWeaponAction = playerInput.actions["Previous"];
        nextWeaponAction = playerInput.actions["Next"];
        rb = GetComponent<Rigidbody2D>();
        //CalculateWeaponCloud();
        CheckForUniqueWeapons();
    }

    // Update is called once per frame
    void Update()
    {

        float horizontaldir = Mathf.Round(moveAction.ReadValue<Vector2>().x);
        float verticaldir = Mathf.Round(moveAction.ReadValue<Vector2>().y);
        moveVector = new Vector2(horizontaldir, verticaldir).normalized;

        if (attackAction.WasPressedThisFrame())
        {
            AttackActiveWeapon();
        }

        if (attackAllAction.WasPressedThisFrame())
        {
            AttackAllWeapons();
        }

        if (throwAction.WasPressedThisFrame())
        {
            ThrowCurrentAndSimilarWeapons();
        }

        if (interactAction.WasPressedThisFrame())
        {
            PickUpNearbyWeapons();
        }

        if (previousWeaponAction.WasPerformedThisFrame())
        {
            ChangeWeaponIndex(weaponIndex - 1, true);
        }

        if (nextWeaponAction.WasPerformedThisFrame())
        {
            ChangeWeaponIndex(weaponIndex + 1, true);
        }
    }

    private void FixedUpdate()
    {
        RotateToMousePosition();
        CheckForNearbyWeapons();
        CalculateWeaponCloud();
        //CALCULATE MOVE SPEED BASED ON SUM OF WEAPON WEIGHTS
        float sumWeights = 0.1f;
        float targetSpeed;
        orbitOffset += orbitSpeed * Time.fixedDeltaTime;
        orbitOffset = orbitOffset % 360;
        foreach (Weapon loopWeapon in weaponList)
        {
            sumWeights += loopWeapon.weight;
        }
        targetSpeed = Mathf.Clamp(moveForce / sumWeights,minMoveSpeed,maxMoveSpeed);
        //Debug.Log(targetSpeed);
        actualSpeed = Mathf.Lerp(rb.linearVelocity.magnitude, targetSpeed, Time.deltaTime * changeVelocitySpeed);

        if (Mathf.Abs(moveVector.magnitude) > 0.1) rb.linearVelocity = moveVector * actualSpeed;

    }

    private void Death()
    {
        Debug.Log("Player Death() called!");
        // TODO: player death animation, sounds

        // TODO: GameManager should handle Game Over!
    }

    public void TakeDamage()
    {
        if (weaponList.Count >= 2)
            DropWeapon(); // drop active weapon
        else
            Death();
    }

    //Fires the active weapon, then changes to a similar weapon if applicable.
    private void AttackActiveWeapon()
    {
        weaponList[weaponIndex].Attack(true);
        if (sameWeaponTypeList.Count > 0)
        {
            int newSameWeaponIndex = sameWeaponTypeList.IndexOf(weaponList[weaponIndex]) + 1;
            if (newSameWeaponIndex >= sameWeaponTypeList.Count) newSameWeaponIndex = 0;
            ChangeWeaponIndex(weaponList.IndexOf(sameWeaponTypeList[newSameWeaponIndex]));
        }
    }

    //Calls the attack function on all weapons that are similar to the active weapon
    private void AttackAllWeapons()
    {
        foreach (Weapon loopWeapon in sameWeaponTypeList)
        {
            loopWeapon.Attack(true);
        }

    }

    //Throws the active and all similar weapons to the active weapon. Called with the throw action
    private void ThrowCurrentAndSimilarWeapons()
    {
        if (weaponIndex == 0) return;
        foreach (Weapon loopWeapon in sameWeaponTypeList)
        {
            loopWeapon.Throw();
        }

        foreach (Weapon loopWeapon in sameWeaponTypeList)
        {
            weaponList.Remove(loopWeapon);
        }
        ChangeWeaponIndex(Mathf.RoundToInt(Random.Range(1,weaponList.Count)));
    }

    //Removes currently active weapon from list, and weapon disappears. Called when player is damaged. If 
    private void DropWeapon()
    {
        //Saves the Sword from being dropped. This is probably not the best way to do it, but the edge cases are prevented
        //by the circumstances of the call so who cares
        bool currentWeaponIsSword = (weaponIndex == 0);
        if (currentWeaponIsSword) weaponIndex += 1;

        if (sameWeaponTypeList.Count >= 1)
        {
            int newSameWeaponIndex = sameWeaponTypeList.IndexOf(weaponList[weaponIndex]) + 1;
            if (newSameWeaponIndex >= sameWeaponTypeList.Count) newSameWeaponIndex = 0;
            weaponList[weaponIndex].Drop();
            weaponList.Remove(weaponList[weaponIndex]);
            ChangeWeaponIndex(weaponList.IndexOf(sameWeaponTypeList[newSameWeaponIndex]));
        }
        else
        {
            weaponList[weaponIndex].Drop();
            weaponList.Remove(weaponList[weaponIndex]);
            ChangeWeaponIndex(weaponIndex);
        }

        if (currentWeaponIsSword) weaponIndex = 0;

    }

    //Removes a specific weapon from list. Called when a weapon runs out of ammo
   public void ThrowWeapon(Weapon weaponToThrow)
    {
        StartCoroutine(ThrowWeaponAfter(weaponToThrow));
    }

    //This is to avoid list changes, causing errors. 
    public IEnumerator ThrowWeaponAfter(Weapon weaponToThrow)
    {
        Debug.Log("ThrowWeaponAfter() called");
        yield return new WaitForEndOfFrame();
        weaponToThrow.Throw();
        weaponList.Remove(weaponToThrow);
        if (sameWeaponTypeList.Contains(weaponToThrow))
        {
            weaponList.Remove(weaponToThrow);
        }
        ChangeWeaponIndex(weaponIndex);
    }

    //Changes the Weapon Index and and calls FindSameWeapon and checks for unique weapons
    private void ChangeWeaponIndex(int changeTo, bool differentFromActive = false)
    {
        //Initiates different behavior if the player is scrolling/changes weapon.
        //Makes it so that the weapon is changed to another of a different type of the active weapon
        if (differentFromActive)
        {
            int uniqueWeaponIndex = uniqueWeaponList.IndexOf(weaponList[weaponIndex].weaponName);
            if (changeTo > weaponIndex) uniqueWeaponIndex += 1;
            else uniqueWeaponIndex -= 1;

            if (uniqueWeaponIndex < 0) uniqueWeaponIndex = uniqueWeaponList.Count - 1;
            if (uniqueWeaponIndex >= uniqueWeaponList.Count) uniqueWeaponIndex = 0;

            foreach (Weapon weapon in weaponList)
            {
                if(weapon.weaponName == uniqueWeaponList[uniqueWeaponIndex])
                {
                    weaponIndex = weaponList.IndexOf(weapon);
                    break;
                }
            }
        }
        else
        {
            weaponIndex = changeTo;
        }
        if (weaponIndex < 0) weaponIndex = weaponList.Count - 1;
        if (weaponIndex >= weaponList.Count) weaponIndex = 0;
        CheckForUniqueWeapons();
        FindSameWeapon(weaponList[weaponIndex]);
    }

    //Populates the sameWeaponTypeList with a list of weapons that have the same name as the active weapon
    private void FindSameWeapon(Weapon activeWeapon)
    {
        sameWeaponTypeList.Clear();
        foreach (Weapon loopWeapon in weaponList)
        {
            if(loopWeapon.weaponName == activeWeapon.weaponName)
            {
                sameWeaponTypeList.Add(loopWeapon);
            }
        }
    }

    //Checks for nearby weapons through a physics overlap circle, only looking at the weapon layer. Adds nearby weapons to list if 
    //There is still ammo inside. 
    private void CheckForNearbyWeapons()
    {
        foreach(Weapon oldNearbyWeapon in nearbyWeaponsList)
        {
            oldNearbyWeapon.TogglePickUpAble(false);
        }
        nearbyWeaponsList.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRange, weaponDetectionLayers);
        Debug.DrawLine(transform.position, transform.position + Vector3.right * pickupRange, Color.white);
        foreach (Collider2D collider in colliders)
        {
            if(collider.TryGetComponent<Weapon>(out Weapon potentialWeapon))
            {
                if(potentialWeapon.curr_ammo > 0)
                {
                    potentialWeapon.TogglePickUpAble(true);
                    nearbyWeaponsList.Add(potentialWeapon);
                }
            }
        }
    }

    //Picks up all nearby weapons. Updates same weapon list. 
    private void PickUpNearbyWeapons()
    {
        foreach (Weapon nearbyWeapon in nearbyWeaponsList)
        {
            nearbyWeapon.TogglePickUpAble(false);
            nearbyWeapon.Pickup();
            weaponList.Add(nearbyWeapon);
            nearbyWeapon.transform.SetParent(weaponHolder);
        }
        FindSameWeapon(weaponList[weaponIndex]);
        nearbyWeaponsList.Clear();
    }

    //Helper Class that creates a list of unique weapons. Called everytime the weapon index changes
    private void CheckForUniqueWeapons()
    {
        uniqueWeaponList.Clear();
        foreach (Weapon weapon in weaponList)
        {
            if (!uniqueWeaponList.Contains(weapon.weaponName))
            {
                uniqueWeaponList.Add(weapon.weaponName);
            }
        }
    }

    //Creates a circle around the player/active weapon at even intervals determined by the number of weapons
    private void CalculateWeaponCloud()
    {
        float angleStep = 0;
        float angleOffset = 0;
        float newWeaponRadius = weaponRevolveRadius;
        //Creates a weapon cloud depending on the number of weapons weapon list
        angleStep = 360f / Mathf.Clamp(weaponList.Count , 0, 7);
        angleOffset = orbitOffset;

        if (weaponList.Count == 1) newWeaponRadius = 0.25f;

        for (int i = 0; i < Mathf.Clamp(weaponList.Count,0,7); i++)
        {
            // Calculate the position of the follower in world space
            Vector3 offset = new Vector3(Mathf.Cos(angleOffset * Mathf.Deg2Rad) * newWeaponRadius, Mathf.Sin(angleOffset * Mathf.Deg2Rad) * newWeaponRadius, 0f);
            angleOffset += angleStep;
            // Update the position of the follower
            weaponList[i].setDestination = transform.position + offset;
        }

        if (weaponList.Count > 7)
        {
            angleStep = 360f / Mathf.Clamp(weaponList.Count - 7 , 0, 12);
            angleOffset = orbitOffset;
            newWeaponRadius = newWeaponRadius * 1.5f;
            for (int i = 7; i < Mathf.Clamp(weaponList.Count, 7, 19); i++)
            {
                // Calculate the position of the follower in world space
                Vector3 offset = new Vector3(Mathf.Cos(angleOffset * Mathf.Deg2Rad) * newWeaponRadius, Mathf.Sin(angleOffset * Mathf.Deg2Rad) * newWeaponRadius, 0f);
                angleOffset += angleStep;
                // Update the position of the follower
                weaponList[i].setDestination = transform.position + offset;
            }
        }
        
        if(weaponList.Count > 19)
        {
            angleStep = 360f / (weaponList.Count - 19);
            angleOffset = orbitOffset;
            newWeaponRadius = newWeaponRadius * 1.5f;
            for (int i = 19; i < weaponList.Count; i++)
            {
                // Calculate the position of the follower in world space
                Vector3 offset = new Vector3(Mathf.Cos(angleOffset * Mathf.Deg2Rad) * newWeaponRadius, Mathf.Sin(angleOffset * Mathf.Deg2Rad) * newWeaponRadius, 0f);
                angleOffset += angleStep ;
                // Update the position of the follower
                weaponList[i].setDestination = transform.position + offset;
            }
        }

        //Sets Active weapon to be centered in the cloud. Also resets the 
        playerCollider.radius = newWeaponRadius;
        weaponList[weaponIndex].setDestination = transform.position;
    }

    private void RotateToMousePosition()
    {
        // Get the mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Calculate the direction from the player to the mouse position
        Vector2 direction = (mousePosition - transform.position).normalized;

        // Calculate the angle between the player's forward direction and the direction to the mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Smoothly rotate the player towards the mouse position
        float step = rotateSpeed * Time.deltaTime;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle -90));
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

        //Rotates all weapons to align with the player
        foreach (Weapon loopWeapon in weaponList)
        {
            loopWeapon.transform.rotation = Quaternion.RotateTowards(loopWeapon.transform.rotation, targetRotation, step);
        }
    }
}
