using UnityEngine;

public class WeaponSimple : MonoBehaviour
{
    // Este script solo necesita estar en el objeto arma
    // La lógica de recogida está en EnemySimple.cs
    
    void Start()
    {
        // Asegurarse de que el arma tenga el tag correcto
        gameObject.tag = "Weapon";
    }
}