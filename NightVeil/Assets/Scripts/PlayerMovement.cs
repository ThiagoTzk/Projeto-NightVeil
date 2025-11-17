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

    [Header("References")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private GameManager gm;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        gm = FindObjectOfType<GameManager>();

        if (gm == null)
            Debug.LogError("PlayerMovement: Nenhum GameManager encontrado na cena!");
    }

    void Update()
    {
        // -----------------------------------------
        //        SISTEMA DE PAUSE (ESC / P)
        // -----------------------------------------
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (!gm.IsPaused) gm.PauseGame();
            else gm.ResumeGame();
        }

        // Se pausado → interrompe movimento e animação
        if (gm.IsPaused)
        {
            if (animator)
                animator.SetFloat("Speed", 0f);

            return;
        }

        // -----------------------------------------
        //          DETECÇÃO DE CHÃO
        // -----------------------------------------
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // -----------------------------------------
        //          MOVIMENTO NORMAL
        // -----------------------------------------
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        controller.Move(move * speed * Time.deltaTime);

        // -----------------------------------------
        //                PULO
        // -----------------------------------------
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // -----------------------------------------
        //             GRAVIDADE
        // -----------------------------------------
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // -----------------------------------------
        //        ANIMAÇÃO DO PERSONAGEM
        // -----------------------------------------
        if (animator)
        {
            float magnitude = new Vector2(x, z).magnitude;
            animator.SetFloat("Speed", magnitude, 0.1f, Time.deltaTime);
            animator.SetBool("IsJumping", !isGrounded);
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
