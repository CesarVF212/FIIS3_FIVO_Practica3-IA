using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // Velocidad de movimiento
    private Rigidbody rb; // Referencia al Rigidbody
    private PlayerInput input; // Referencia al sistema de entrada
    private Vector2 movementInput; // Almacena la entrada de movimiento

    void Awake()
    {
        rb = GetComponent<Rigidbody>(); // Obtiene el componente Rigidbody
        input = GetComponent<PlayerInput>(); // Obtiene el componente PlayerInput
    }

    void OnEnable()
    {
        // Suscribirse a los eventos de entrada
        input.actions["Move"].performed += OnMove;
        input.actions["Move"].canceled += OnMove;

        if (input.camera == null)
            Debug.Log("No se ha añadido la cámara al PlayerInput");
    }

    void OnDisable()
    {
        // Desuscribirse de los eventos de entrada
        input.actions["Move"].performed -= OnMove;
        input.actions["Move"].canceled -= OnMove;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>(); // Lee la entrada de movimiento
    }

    void FixedUpdate()
    {
        if (movementInput == Vector2.zero) return; // Si no hay entrada, no se mueve

        // Obtener la cámara desde el PlayerInput
        Camera cam = Camera.main; // Cambiado para obtener la cámara principal
        if (cam == null) return;

        // Dirección adelante y derecha en el plano XZ
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0; // Ignorar la componente Y
        camForward.Normalize();

        Vector3 camRight = cam.transform.right;
        camRight.y = 0; // Ignorar la componente Y
        camRight.Normalize();

        // Movimiento en función de la cámara
        Vector3 moveDirection = camForward * movementInput.y + camRight * movementInput.x;
        moveDirection.Normalize();

        // Mover el Rigidbody
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }
}
