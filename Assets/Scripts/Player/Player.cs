using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Singleton<Player>
{

    //Player.Instance.Initialize();

    [SerializeField] float MOVE_SPEED = 30f;
    [SerializeField] float changeVelocitySpeed;
    float actualSpeed;
    Vector2 moveVector;
    Vector2 cursorPos;
    [SerializeField] PlayerInput playerInput;
    InputAction moveAction; InputAction shootAction; InputAction lookAction; InputAction interactAction; InputAction throwAction; InputAction previousWeaponAction; InputAction nextWeaponAction;
    Rigidbody2D rb;

    [SerializeField] List<Weapon> weaponList = new List<Weapon>();
    [SerializeField] int weaponIndex = 0;

    void Initialize()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Shoot"];
        throwAction = playerInput.actions["Throw"];
        interactAction = playerInput.actions["Interact"];
        previousWeaponAction = playerInput.actions["Interact"];
        nextWeaponAction = playerInput.actions["Interact"];
        rb = GetComponent<Rigidbody2D>();
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

        if (shootAction.WasPressedThisFrame())
        {
            AttackWeapon();
        }

        if (throwAction.WasPressedThisFrame())
        {

        }

        if (interactAction.WasPressedThisFrame())
        {

        }

        if (previousWeaponAction.WasPressedThisFrame())
        {
            ChangeWeaponIndex(weaponIndex - 1);
        }

        if (nextWeaponAction.WasPressedThisFrame())
        {
            ChangeWeaponIndex(weaponIndex + 1);
        }
    }

    private void FixedUpdate()
    {
        //CALCULATE MOVE SPEED BASED ON SUM OF WEAPON WEIGHTS
        actualSpeed = Mathf.Lerp(rb.linearVelocity.magnitude, MOVE_SPEED, Time.deltaTime * changeVelocitySpeed);

        if (Mathf.Abs(moveVector.magnitude) > 0.1) rb.linearVelocity = moveVector * actualSpeed;
        
    }

    private void AttackWeapon()
    {
        weaponList[weaponIndex].Attack();
    }

    private void ChangeWeaponIndex(int changeTo)
    {
        weaponIndex = changeTo;
        if(weaponIndex < 0) weaponIndex = weaponList.Count - 1;
        if (weaponIndex >= weaponList.Count) weaponIndex = 0;
    }
}
