using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configurações de Ataque")]
    public int damage = 50;
    public float attackRange = 3f;
    public KeyCode attackKey = KeyCode.Mouse0;
    public LayerMask zombieLayer;

    private GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (gameManager == null || !gameManager.IsPlaying || gameManager.IsPaused)
            return;

        if (Input.GetKeyDown(attackKey))
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider[] hitZombies = Physics.OverlapSphere(transform.position, attackRange, zombieLayer);

        foreach (Collider zombie in hitZombies)
        {
            ZombieSimple zombieScript = zombie.GetComponent<ZombieSimple>();
            if (zombieScript != null)
            {
                zombieScript.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}