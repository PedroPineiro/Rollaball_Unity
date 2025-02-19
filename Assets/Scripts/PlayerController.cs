
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Referencia al Rigidbody del jugador
    private Rigidbody rb;

    public Camera thirdPersonCamera;
    public Camera firstPersonCamera;

    
    // Referencia al Animator del jugador
    private Animator controlLevel;

    // Variable para rastrear el estado actual del Animator
    private string currentState;

    // Contador de pickups recogidos
    private int count;

    // Variables de movimiento en los ejes X e Y
    private float movementX;
    private float movementY;

    // Velocidad de movimiento del jugador
    public float speed = 0;

    // Altura mínima antes de considerar que el jugador ha muerto
    public float positionY = -15f;

    // UI: Texto que muestra la cantidad de pickups recogidos
    public TextMeshProUGUI countText;

    // UI: Objeto de texto que muestra el mensaje de victoria o derrota
    public GameObject winTextObject;

    private Transform playerTransform;


    void Start()
    {
        // Obtiene el componente Rigidbody del jugador
        rb = GetComponent<Rigidbody>();

        // Inicializa el contador de pickups en 0
        count = 0;
        SetPickups(); // Actualiza la UI con el conteo inicial

        // Desactiva el mensaje de victoria al inicio del juego
        winTextObject.SetActive(false);

        // Obtiene el componente Animator del jugador
        controlLevel = GetComponent<Animator>();


        playerTransform = GetComponent<Transform>();

        // Establece el estado inicial en "Level_1"
        currentState = "Level_1";

        thirdPersonCamera.enabled = true;
        firstPersonCamera.enabled = false;
        
        Input.gyro.enabled = true;

    }

    // Maneja la entrada de movimiento del jugador
    void OnMove(InputValue movementValue)
    {
        // Convierte la entrada en un vector de dos dimensiones
        Vector2 movementVector = movementValue.Get<Vector2>();

        // Almacena los valores de movimiento en X e Y
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    private void Update()
    {
        // Verifica si el jugador ha caído por debajo del límite permitido
        if (transform.position.y <= positionY)
        {
            Die(); // Ejecuta la función de muerte
        }

        // Obtiene el estado actual del Animator
        AnimatorStateInfo stateInfo = controlLevel.GetCurrentAnimatorStateInfo(0);

        // Si el estado cambia a Level_2 y aún no está en ese estado
        if (stateInfo.IsName("Level_2") && currentState != "Level_2")
        {
            currentState = "Level_2"; // Actualiza el estado
            DestroyObjects("Enemy_1", "Valla_Level_1"); // Destruye los obstáculos del nivel 1
        }
        // Si el estado cambia a Level_3 y aún no está en ese estado
        else if (stateInfo.IsName("Level_3") && currentState != "Level_3")
        {
            currentState = "Level_3"; // Actualiza el estado
            DestroyObjects("Enemy_2", "Valla_Level_2"); // Destruye los obstáculos del nivel 2
        }

        // Cambiar camara (1 a 3)
        if (Input.GetKeyDown(KeyCode.F))
        {
            thirdPersonCamera.enabled = !thirdPersonCamera.enabled;
            firstPersonCamera.enabled = !firstPersonCamera.enabled;

        }
    }

    // Maneja la física del movimiento del jugador
    private void FixedUpdate()
    {
        if (firstPersonCamera.enabled)
        {
            Vector3 cameraForward = firstPersonCamera.transform.forward;
            Vector3 cameraRight = firstPersonCamera.transform.right;

            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            Vector3 movement = cameraForward * movementY + cameraRight * movementX;
            rb.AddForce(movement * speed);
        }
        else if (thirdPersonCamera.enabled)
        {
            Vector3 movement = new Vector3(movementX, 0.0f, movementY);
            rb.AddForce(movement * speed);

            if (SystemInfo.supportsGyroscope)
            {
                Quaternion deviceRotation = Input.gyro.attitude;
                Vector3 gyroMovement = new Vector3(-deviceRotation.y, 0, deviceRotation.x); // Ajustar los ejes según la orientación

                // Ajustar la sensibilidad
                gyroMovement *= speed * Time.deltaTime;

                // Aplicar el movimiento con el giroscopio
                playerTransform.Translate(gyroMovement);
            }
        }
    }

    // Maneja las colisiones con otros objetos
    void OnTriggerEnter(Collider other)
    {
        // Si el jugador recoge un objeto con la etiqueta "PickUp"
        if (other.gameObject.CompareTag("PickUp"))
        {
            // Desactiva el objeto recogido
            other.gameObject.SetActive(false);

            // Incrementa el contador de pickups
            count++;

            // Actualiza la UI con el nuevo conteo de pickups
            SetPickups();
        }

        // Si el jugador llega a la meta
        if (other.gameObject.CompareTag("Meta"))
        {
            // Muestra el mensaje de victoria
            winTextObject.SetActive(true);
            winTextObject.GetComponent<TextMeshProUGUI>().text = "You Win!";

            // Destruye al jugador para evitar más movimiento
            Destroy(gameObject);
        }
    }

    // Función para actualizar el texto de pickups y cambiar estados del Animator
    void SetPickups()
    {
        // Actualiza el texto con la cantidad de pickups recogidos
        countText.text = "Count: " + count.ToString();

        // Si el jugador ha recogido 10 pickups, activa la transición a Level_2
        if (count >= 10)
        {
            controlLevel.SetBool("Pickups_Level_1", true);
        }

        // Si el jugador ha recogido 20 pickups, activa la transición a Level_3
        if (count >= 20)
        {
            controlLevel.SetBool("Pickups_Level_2", true);
        }
    }

    // Maneja las colisiones con los enemigos
    private void OnCollisionEnter(Collision collision)
    {
        // Si el jugador toca un enemigo, muere
        if (collision.gameObject.CompareTag("Enemy_1") || collision.gameObject.CompareTag("Enemy_2"))
        {
            Die();
        }
    }

    // Función para manejar la muerte del jugador
    private void Die()
    {
        // Destruye al jugador
        Destroy(gameObject);

        // Muestra el mensaje de derrota en pantalla
        winTextObject.SetActive(true);
        winTextObject.GetComponent<TextMeshProUGUI>().text = "You Lose!";
    }

    // Función auxiliar para destruir objetos por etiqueta
    private void DestroyObjects(string enemyTag, string vallaTag)
    {
        // Encuentra y destruye al enemigo si existe
        GameObject enemy = GameObject.FindGameObjectWithTag(enemyTag);
        if (enemy) Destroy(enemy);

        // Encuentra y destruye la valla si existe
        GameObject valla = GameObject.FindGameObjectWithTag(vallaTag);
        if (valla) Destroy(valla);
    }
}
