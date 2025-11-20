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

        // === TESTES TEMPORÁRIOS DE HUD ===
        if (Input.GetKeyDown(KeyCode.T)) // Tecla T para tomar dano
        {
            gm.PlayerTakeDamage(10);
            Debug.Log("Tomou 10 de dano! Vida atual: " + gm.GetPlayerHealth());
        }

        if (Input.GetKeyDown(KeyCode.H)) // Tecla H para curar
        {
            // Método temporário para curar
            int currentHealth = gm.GetPlayerHealth();
            currentHealth = Mathf.Min(currentHealth + 10, 100);
            UpdatePlayerHealth(currentHealth);
            Debug.Log("Curou 10! Vida atual: " + currentHealth);
        }

        if (Input.GetKeyDown(KeyCode.Y)) // Tecla Y para adicionar pontos
        {
            gm.AddPoints(100);
            Debug.Log("Adicionou 100 pontos!");
        }

        if (Input.GetKeyDown(KeyCode.U)) // Tecla U para próxima wave
        {
            gm.NextWave();
            Debug.Log("Próxima wave: " + gm.GetCurrentWave());
        }

        if (Input.GetKeyDown(KeyCode.I)) // Tecla I para gastar munição
        {
            int currentAmmo = GetCurrentAmmo();
            int maxAmmo = GetMaxAmmo();

            if (currentAmmo > 0)
            {
                UpdateAmmo(currentAmmo - 1, maxAmmo);
                Debug.Log("Atirou! Munição: " + (currentAmmo - 1) + "/" + maxAmmo);
            }
            else
            {
                Debug.Log("Sem munição!");
            }
        }

        if (Input.GetKeyDown(KeyCode.O)) // Tecla O para recarregar
        {
            int currentAmmo = GetCurrentAmmo();
            int maxAmmo = GetMaxAmmo();

            UpdateAmmo(maxAmmo, maxAmmo);
            Debug.Log("Recarregou! Munição: " + maxAmmo + "/" + maxAmmo);
        }

        if (Input.GetKeyDown(KeyCode.R)) // Tecla R para resetar testes
        {
            // Reset temporário para testes
            UpdatePlayerHealth(100);
            int maxAmmo = GetMaxAmmo();
            UpdateAmmo(maxAmmo, maxAmmo);
            Debug.Log("Resetou vida e munição!");
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

    // Métodos temporários para testes (usando reflection)
    private void UpdatePlayerHealth(int newHealth)
    {
        System.Reflection.FieldInfo healthField = typeof(GameManager).GetField("playerHealth",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (healthField != null)
        {
            healthField.SetValue(gm, newHealth);
            gm.UpdateHUD();
        }
    }

    private int GetCurrentAmmo()
    {
        System.Reflection.FieldInfo ammoField = typeof(GameManager).GetField("currentAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (ammoField != null)
            return (int)ammoField.GetValue(gm);

        return 0;
    }

    private int GetMaxAmmo()
    {
        System.Reflection.FieldInfo ammoField = typeof(GameManager).GetField("maxAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (ammoField != null)
            return (int)ammoField.GetValue(gm);

        return 0;
    }

    private void UpdateAmmo(int current, int max)
    {
        System.Reflection.FieldInfo currentAmmoField = typeof(GameManager).GetField("currentAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        System.Reflection.FieldInfo maxAmmoField = typeof(GameManager).GetField("maxAmmo",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (currentAmmoField != null && maxAmmoField != null)
        {
            currentAmmoField.SetValue(gm, current);
            maxAmmoField.SetValue(gm, max);
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