using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Singleton<Player>
{

    [SerializeField] float moveSpeed = 30f;
    [SerializeField] float changeVelocitySpeed;
    float actualSpeed;
    [SerializeField] float rotateSpeed = 10f;
    Vector2 moveVector;
    Vector2 cursorPos;
    [SerializeField] PlayerInput playerInput;
    InputAction moveAction; InputAction attackAction; InputAction attackAllAction; InputAction interactAction; InputAction throwAction; InputAction previousWeaponAction; InputAction nextWeaponAction;
    Rigidbody2D rb;

    [SerializeField] List<Weapon> weaponList = new List<Weapon>(); //This is the core of the class. Contains a list of all weapons avaliable to the player
    [SerializeField] int weaponIndex = 0; //Index of the active weapon. Change this to change teh active weapon.
    [SerializeField] float weaponRevolveRadius;
    [SerializeField] List<Weapon> sameWeaponTypeList = new List<Weapon>(); //List of weapons that are the same as the active weapon. Recalcalculated whenever the active weapon changes
    [SerializeField] List<Weapon> nearbyWeaponsList = new List<Weapon>(); //List of weapons that are nearby

    [SerializeField] float pickupRange;
    [SerializeField] LayerMask weaponDetectionLayers;

    void Initialize()
    {

    }

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
        CalculateWeaponCloud();
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
            ThowCurrentAndSimilarWeapons();
        }

        if (interactAction.WasPressedThisFrame())
        {
            PickUpNearbyWeapons();
        }

        if (previousWeaponAction.WasPerformedThisFrame())
        {
            ChangeWeaponIndex(weaponIndex - 1);
        }

        if (nextWeaponAction.WasPerformedThisFrame())
        {
            ChangeWeaponIndex(weaponIndex + 1);
        }
    }

    private void FixedUpdate()
    {
        RotateToMousePosition();
        CheckForNearbyWeapons();
        CalculateWeaponCloud();
        //CALCULATE MOVE SPEED BASED ON SUM OF WEAPON WEIGHTS
        actualSpeed = Mathf.Lerp(rb.linearVelocity.magnitude, moveSpeed, Time.deltaTime * changeVelocitySpeed);

        if (Mathf.Abs(moveVector.magnitude) > 0.1) rb.linearVelocity = moveVector * actualSpeed;

    }

    //Fires the active weapon, then changes to a similar weapon if applicable.
    private void AttackActiveWeapon()
    {
        weaponList[weaponIndex].Attack(true);
        if(sameWeaponTypeList.Count > 0)
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
    private void ThowCurrentAndSimilarWeapons()
    {

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

    //Removes a weapon from the weapon list at random. Called when player is damaged
    private void ThrowRandomWeapon()
    {
        int randomIndex = Mathf.RoundToInt(Random.Range(1, weaponList.Count));
        weaponList[randomIndex].Throw();
        weaponList.Remove(weaponList[randomIndex]);
        if (randomIndex == weaponIndex) ChangeWeaponIndex(randomIndex);
    }

    //Removes a specific weapon from list. Called when a weapon runs out of ammo
   public void ThrowWeapon(Weapon weaponToThrow)
    {
        StartCoroutine(ThrowWeaponAfter(weaponToThrow));
    }

    //This is to avoid list changes, causing errors. 
    public IEnumerator ThrowWeaponAfter(Weapon weaponToThrow)
    {
        yield return new WaitForEndOfFrame();
        weaponToThrow.Throw();
        weaponList.Remove(weaponToThrow);
        if (sameWeaponTypeList.Contains(weaponToThrow))
        {
            weaponList.Remove(weaponToThrow);
        }
        ChangeWeaponIndex(weaponIndex);
    }

    //Changes the Weapon Index and and calls FindSameWeapon
    private void ChangeWeaponIndex(int changeTo)
    {
        weaponIndex = changeTo;
        if (weaponIndex < 0) weaponIndex = weaponList.Count - 1;
        if (weaponIndex >= weaponList.Count) weaponIndex = 0;
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
            Debug.Log(collider.name);
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

    private void PickUpNearbyWeapons()
    {
        foreach (Weapon nearbyWeapon in nearbyWeaponsList)
        {
            nearbyWeapon.TogglePickUpAble(false);
            nearbyWeapon.Pickup();
            weaponList.Add(nearbyWeapon);
        }
        FindSameWeapon(weaponList[weaponIndex]);
        nearbyWeaponsList.Clear();
    }

    //Creates a circle around the player/active weapon at even intervals determined by the number of weapons
    private void CalculateWeaponCloud()
    {
        float angleStep = 360f / weaponList.Count;
        float angleOffset = 0;
        for (int i = 0; i<weaponList.Count; i++)
        {
            // Calculate the position of the follower in world space
            Vector3 offset = new Vector3(Mathf.Cos(angleOffset * Mathf.Deg2Rad) * weaponRevolveRadius, Mathf.Sin(angleOffset * Mathf.Deg2Rad) * weaponRevolveRadius, 0f);
            angleOffset += angleStep;
            // Update the position of the follower
            weaponList[i].setDestination = transform.position + offset;
        }

        //Sets Active weapon to be centered in the cloud
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
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

        //Rotates all weapons to align with the player
        foreach (Weapon loopWeapon in weaponList)
        {
            loopWeapon.transform.rotation = targetRotation;
        }
    }
}
