using NUnit.Framework;
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

    [SerializeField] List<Weapon> weaponList = new List<Weapon>();
    [SerializeField] int weaponIndex = 0;
    [SerializeField] float weaponRevolveRadius;
    [SerializeField] List<Weapon> sameWeaponTypeList = new List<Weapon>();

    [SerializeField] float pickupRange;
    [SerializeField] LayerMask detectionLayers;

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
        CalculateWeaponCloud();
        //CALCULATE MOVE SPEED BASED ON SUM OF WEAPON WEIGHTS
        actualSpeed = Mathf.Lerp(rb.linearVelocity.magnitude, moveSpeed, Time.deltaTime * changeVelocitySpeed);

        if (Mathf.Abs(moveVector.magnitude) > 0.1) rb.linearVelocity = moveVector * actualSpeed;

    }
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

    private void AttackAllWeapons()
    {
        foreach (Weapon loopWeapon in sameWeaponTypeList)
        {
            loopWeapon.Attack(true);
        }

    }

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

    private void ThrowRandomWeapon()
    {
        int randomIndex = Mathf.RoundToInt(Random.Range(1, weaponList.Count));
        weaponList[randomIndex].Throw();
        weaponList.Remove(weaponList[randomIndex]);
        if (randomIndex == weaponIndex) ChangeWeaponIndex(randomIndex);
    }

    private void ChangeWeaponIndex(int changeTo)
    {
        weaponIndex = changeTo;
        if (weaponIndex < 0) weaponIndex = weaponList.Count - 1;
        if (weaponIndex >= weaponList.Count) weaponIndex = 0;
        FindSameWeapon(weaponList[weaponIndex]);
    }

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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, pickupRange, detectionLayers);
        Debug.DrawLine(transform.position, transform.position + Vector3.right * pickupRange, Color.white);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Weapon"))
            {

            }
        }
    }

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
