using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;                // arraste o objeto Player (root) aqui
    public float mouseSensitivity = 200f;   // ajuste fino no Inspector (200 é razoável)
    [Range(0f, 90f)]
    public float pitchLimit = 85f;          // limite vertical

    float pitch = 0f; // rotação vertical acumulada (em graus)

    void Awake()
    {
        if (player == null)
            Debug.LogError("FirstPersonCamera: arraste o Player (root) no campo 'player' do Inspector.");
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // leitura do mouse (frame-independent)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // pitch (vertical) aplicado apenas na câmera (este GameObject)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // yaw (horizontal) aplicado no player root — isso gira o corpo inteiro
        if (player != null)
            player.Rotate(Vector3.up * mouseX);
    }
}
