using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.Jobs;
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
    InputAction moveAction; InputAction attackAction; InputAction lookAction; InputAction interactAction; InputAction throwAction; InputAction previousWeaponAction; InputAction nextWeaponAction;
    Rigidbody2D rb;

    [SerializeField] List<Weapon> weaponList = new List<Weapon>();
    [SerializeField] int weaponIndex = 0;
    [SerializeField] float weaponRevolveRadius;

    void Initialize()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        attackAction = playerInput.actions["Attack"];
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

        if (lookAction.WasPressedThisFrame())
        {

        }

        if (attackAction.WasPressedThisFrame())
        {
            AttackWeapon();
        }

        if (throwAction.WasPressedThisFrame())
        {
            ThrowCurrentWeapon();
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

        //CALCULATE MOVE SPEED BASED ON SUM OF WEAPON WEIGHTS
        actualSpeed = Mathf.Lerp(rb.linearVelocity.magnitude, moveSpeed, Time.deltaTime * changeVelocitySpeed);

        if (Mathf.Abs(moveVector.magnitude) > 0.1) rb.linearVelocity = moveVector * actualSpeed;

    }
    private void AttackWeapon()
    {
        //weaponList[weaponIndex].Attack();
    }

    private void ThrowCurrentWeapon()
    {
        weaponList[weaponIndex].transform.SetParent(null);
        weaponList[weaponIndex].Throw();
        weaponList.Remove(weaponList[weaponIndex]);
        ChangeWeaponIndex(weaponIndex);
    }

    private void ChangeWeaponIndex(int changeTo)
    {
        weaponIndex = changeTo;
        if (weaponIndex < 0) weaponIndex = weaponList.Count - 1;
        if (weaponIndex >= weaponList.Count) weaponIndex = 0;
        CalculateWeaponCloud();
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
    }
}
