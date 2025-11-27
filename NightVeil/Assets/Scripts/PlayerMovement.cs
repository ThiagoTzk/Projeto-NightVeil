using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 1.2f;
    public float gravity = -30f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.25f;
    public LayerMask groundMask;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private GameManager gm;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        gm = FindObjectOfType<GameManager>();

        if (gm == null)
            Debug.LogError("PlayerMovement: Nenhum GameManager encontrado!");
        else
            Debug.Log("PlayerMovement: GameManager ENCONTRADO!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (!gm.IsPaused) gm.PauseGame();
            else gm.ResumeGame();
        }

        if (gm.IsPaused) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            gm.PlayerTakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            UpdatePlayerHealth(Mathf.Min(gm.GetPlayerHealth() + 10, 100));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ZombieSimple[] zombies = FindObjectsOfType<ZombieSimple>();
            foreach (ZombieSimple zombie in zombies)
            {
                zombie.TakeDamage(1000);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            gm.UpdateAmmo(30, 120);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            gm.AddPoints(100);
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdatePlayerHealth(int newHealth)
    {
        var healthField = typeof(GameManager).GetField("playerHealth",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (healthField != null)
        {
            healthField.SetValue(gm, newHealth);
            gm.UpdateHUD();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}