using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform player;    // Arraste o Player Root aqui

    [Header("Camera Settings")]
    public float pitchLimit = 85f;

    private float pitch = 0f;
    private GameManager gm;

    void Awake()
    {
        gm = FindObjectOfType<GameManager>();

        if (player == null)
            Debug.LogError("FirstPersonCamera: arraste o Player (root) no campo 'player'.");
        if (gm == null)
            Debug.LogError("FirstPersonCamera: nenhum GameManager encontrado na cena!");
    }

    void Update()
    {
        // Se o jogo estiver pausado → não mover câmera
        if (gm == null || gm.IsPaused)
            return;

        // Obtém sensibilidade direto do GameManager
        float sensitivity = gm.Sensitivity;

        // Captura do mouse
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        // Eixo vertical (pitch)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Eixo horizontal (yaw)
        if (player != null)
            player.Rotate(Vector3.up * mouseX);
    }
}
