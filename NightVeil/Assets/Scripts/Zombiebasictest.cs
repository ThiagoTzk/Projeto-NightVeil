using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ZombieSimple : MonoBehaviour
{
    [Header("Configurações Básicas")]
    public float moveSpeed = 2f;
    public float runSpeed = 3.5f;
    public float stoppingDistance = 2f;
    public float attackRange = 2f;
    public float detectionRange = 15f;
    public int damage = 25;
    public float attackCooldown = 1.5f;

    [Header("Vida do Zumbi")]
    public int maxHealth = 125;
    public int currentHealth;

    [Header("Referências")]
    public Animator animator;

    private Transform player;
    private NavMeshAgent agent;
    private GameManager gameManager;
    private float lastAttackTime;
    private bool isDead = false;
    private bool isChasing = false;
    private bool agentValid = false;

    void Start()
    {
        currentHealth = maxHealth;

        agent = GetComponent<NavMeshAgent>();
        gameManager = GameManager.Instance;

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;

            // Verifica se o agent é válido
            agentValid = agent.isActiveAndEnabled && agent.isOnNavMesh;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Encontra o player depois de um pequeno delay para garantir que tudo está inicializado
        Invoke("FindPlayer", 0.5f);
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // Atualiza o estado do agent
        if (agent != null)
        {
            agentValid = agent.isActiveAndEnabled && agent.isOnNavMesh;
        }

        if (gameManager != null && gameManager.IsPaused)
        {
            // Para o movimento apenas se o agent for válido
            if (agentValid)
            {
                agent.isStopped = true;
            }
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (!isChasing)
            {
                isChasing = true;
            }

            FollowPlayer();

            if (distanceToPlayer <= attackRange)
            {
                CheckAttack();
            }
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                StopMovement();
            }
        }
    }

    void FollowPlayer()
    {
        if (player == null || !agentValid) return;

        agent.isStopped = false;
        agent.speed = (Vector3.Distance(transform.position, player.position) < 5f) ? runSpeed : moveSpeed;
        agent.SetDestination(player.position);
        UpdateAnimation();
    }

    void StopMovement()
    {
        if (agentValid)
        {
            agent.isStopped = true;
            UpdateAnimation();
        }
    }

    void CheckAttack()
    {
        if (player == null) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
        lastAttackTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (gameManager != null)
        {
            gameManager.PlayerTakeDamage(damage);
        }

        StartCoroutine(AttackFeedback());
    }

    IEnumerator AttackFeedback()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color[] originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] = renderers[i].material.color;
                renderers[i].material.color = Color.red;
            }
        }

        yield return new WaitForSeconds(0.1f);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].material.color = originalColors[i];
        }
    }

    void UpdateAnimation()
    {
        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude / agent.speed;
            animator.SetFloat("Speed", speed);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        if (animator != null)
        {
            animator.SetTrigger("TakeDamage");
        }

        StartCoroutine(DamageFeedback());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            moveSpeed += 0.5f;
            runSpeed += 0.5f;
        }
    }

    IEnumerator DamageFeedback()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Color originalColor = meshRenderer.material.color;
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            meshRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        isDead = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (gameManager != null)
        {
            gameManager.AddPoints(50);
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }

        Destroy(gameObject, 3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);

        if (Application.isPlaying && player != null && isChasing)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
}