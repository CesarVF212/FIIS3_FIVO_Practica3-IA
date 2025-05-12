using UnityEngine;

public class PlayerSimple : MonoBehaviour
{
    public float velocidad = 5f;
    
    void Update()
    {
        // Movimiento WASD
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movimiento = new Vector3(horizontal, 0, vertical);
        transform.Translate(movimiento * velocidad * Time.deltaTime);
        
        // Rotar hacia el movimiento
        if (movimiento.magnitude > 0.1f)
        {
            transform.LookAt(transform.position + movimiento);
        }
    }
}