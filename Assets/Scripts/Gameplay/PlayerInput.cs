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

        if (Mathf.Abs(horizontal) > 0 || Mathf.Abs(vertical) > 0)
        {
            gameManager.isMoving = true;
        }
        else
        {
            gameManager.isMoving = false;
        }
    }



    void FixedUpdate()
    {
        player.Move(new Vector2(horizontal,vertical), lookTarget);
        player.canShoot = Input.GetMouseButton(0);
        if(Input.GetMouseButtonDown(1)){
            player.ActivateNuke();
        }
    }
}
