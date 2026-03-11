using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private Player player;
    private float horizontal, vertical;
    private Vector2 lookTarget;
    private GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
        gameManager = GameManager.GetInstance();
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        lookTarget = Input.mousePosition;

        HandleWeaponSelectionInput();

        // Handle nuke input - changed to N key
        if(Input.GetKeyDown(KeyCode.N)){
            player.ActivateNuke();
        }

        // Handle shield input with Q key 
        if(Input.GetKeyDown(KeyCode.Q)){
            player.ActivateShield();
        }

        // Handle magnet input with Ctrl key
        if(Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)){
            player.ActivateMagnet();
        }

        // Handle boost input with Shift key
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)){
            player.StartBoost();
        } else {
            player.StopBoost();
        }

        // Handle upgrade menu with T key
        if(Input.GetKeyDown(KeyCode.T)){
            player.ToggleUpgradeMenu();
        }

        // Debug refill for weapon testing
        if (player.IsDebugRefillHotkeyEnabled() && Input.GetKeyDown(KeyCode.R))
        {
            player.RefillAllWeaponsForTesting();
        }

        if (Mathf.Abs(horizontal) > 0 || Mathf.Abs(vertical) > 0)
        {
            gameManager.isMoving = true;
        }
        else
        {
            gameManager.isMoving = false;
        }
    }

    private void HandleWeaponSelectionInput()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
        {
            player.SelectNextWeapon(1);
        }
        else if (scroll < 0f)
        {
            player.SelectNextWeapon(-1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) player.SelectWeaponBySlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) player.SelectWeaponBySlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) player.SelectWeaponBySlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) player.SelectWeaponBySlot(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) player.SelectWeaponBySlot(5);
    }

    void FixedUpdate()
    {
        player.Move(new Vector2(horizontal,vertical), lookTarget);
        player.canShoot = Input.GetMouseButton(0);
    }
}
