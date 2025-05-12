using UnityEngine;

public class EnemySimple : MonoBehaviour
{
    [Header("Configuración")]
    public Transform player;
    public float rangoDeteccion = 10f;
    public bool tengoArma = false;
    public int municion = 0;
    public float rangoAtaque = 1.5f;  // Rango para atacar con puños
    
    [Header("Arma Visual")]
    public GameObject armaVisualMano;  // El arma que aparece en la mano (SEPARADA)
    public Transform puntoDisparo;     // Punto desde donde sale el disparo (punta del arma)
    
    [Header("Sistema de Disparo")]
    public LineRenderer lineRenderer;  // Para dibujar el rayo láser
    public float tiempoRayoVisible = 0.2f;  // Tiempo que el rayo permanece visible
    
    // Variables para buscar armas
    private bool buscandoArma = false;
    private Transform armaObjectivo = null;
    private bool armaSiempreVisible = false;  // Nueva variable para mantener el arma siempre
    
    // Variables para el sistema de disparo
    private bool mostrandoRayo = false;
    private float timerRayo = 0f;
   
    void Start()
    {
        // Buscar al jugador automáticamente
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Configurar el arma visual inicial
        ConfigurarArmaVisual();
        
        // Configurar el LineRenderer para el rayo láser
        ConfigurarLineRenderer();
    }
    
    void ConfigurarLineRenderer()
    {
        if (lineRenderer == null)
        {
            // Crear LineRenderer si no existe
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configurar el LineRenderer
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false; 
    }
    
    void ConfigurarArmaVisual()
    {
        // Mostrar u ocultar el arma según si la tiene o no
        if (armaVisualMano != null)
        {
            // Si ya ha recogido el arma una vez, siempre visible
            if (armaSiempreVisible)
            {
                armaVisualMano.SetActive(true);
                Debug.Log("Arma en mano: SIEMPRE VISIBLE");
            }
            else
            {
                armaVisualMano.SetActive(tengoArma);
                Debug.Log("Arma en mano: " + (tengoArma ? "VISIBLE" : "OCULTA"));
            }
        }
    }
   
    void Update()
    {
        // Ejecutar el árbol de decisión cada frame
        EjecutarArbolDecision();
        
        // Verificar si está cerca del jugador para atacar
        VerificarAtaqueAlJugador();
        
        // Actualizar el rayo láser
        ActualizarRayoLaser();
    }
    
    void ActualizarRayoLaser()
    {
        if (mostrandoRayo)
        {
            timerRayo += Time.deltaTime;
            
            if (timerRayo >= tiempoRayoVisible)
            {
                // Ocultar el rayo
                lineRenderer.enabled = false;
                mostrandoRayo = false;
                timerRayo = 0f;
            }
        }
    }
    
    void VerificarAtaqueAlJugador()
    {
        if (player == null) return;
        
        float distancia = Vector3.Distance(transform.position, player.position);
        if (distancia <= rangoAtaque)
        {
            // El enemigo toca al jugador - simular muerte
            Debug.Log("¡El enemigo ha tocado al jugador!");
            player.gameObject.SetActive(false);
        }
    }
   
    void EjecutarArbolDecision()
    {
        // ¿ENEMIGO EN RANGO?
        if (EnemigoEnRango())
        {
            // Si detectamos al jugador y no tenemos arma, buscar una
            if (!tengoArma && !buscandoArma)
            {
                EncontrarArmaMasCercana();
            }
            
            // Si estamos buscando un arma, ir hacia ella
            if (buscandoArma && armaObjectivo != null)
            {
                IrHaciaArma();
                return; // No hacer nada más hasta coger el arma
            }
            
            // ¿TENGO ARMA?
            if (tengoArma || armaSiempreVisible)  // También contar como "tengo arma" si es siempre visible
            {
                // ¿TENGO MUNICIÓN?
                if (TengoMunicion())
                {
                    Disparar();
                }
                else
                {
                    AtacarPunos();
                }
            }
            else
            {
                // Si no tenemos arma y no estamos buscando una, atacar con puños
                AtacarPunos();
            }
        }
        else
        {
            // Si no hay jugador cerca, resetear búsqueda de arma
            buscandoArma = false;
            armaObjectivo = null;
            Patrullaje();
        }
    }
    
    void EncontrarArmaMasCercana()
    {
        GameObject[] armas = GameObject.FindGameObjectsWithTag("Weapon");
        float distanciaMinima = float.MaxValue;
        GameObject armaMasCercana = null;
        
        foreach (GameObject arma in armas)
        {
            if (!arma.activeInHierarchy) continue; // Ignorar armas ya recogidas
            
            float distancia = Vector3.Distance(transform.position, arma.transform.position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                armaMasCercana = arma;
            }
        }
        
        if (armaMasCercana != null)
        {
            armaObjectivo = armaMasCercana.transform;
            buscandoArma = true;
            Debug.Log("¡Encontrada arma en el suelo! Yendo hacia ella...");
        }
    }
    
    void IrHaciaArma()
    {
        if (armaObjectivo == null || !armaObjectivo.gameObject.activeInHierarchy)
        {
            // El arma desapareció, resetear
            buscandoArma = false;
            armaObjectivo = null;
            return;
        }
        
        Vector3 direccion = (armaObjectivo.position - transform.position).normalized;
        
        // Moverse hacia el arma
        transform.position += direccion * 3f * Time.deltaTime;
        
        // Mirar hacia el arma
        Vector3 direccionMirada = direccion;
        direccionMirada.y = 0; // Solo rotar en Y
        transform.LookAt(transform.position + direccionMirada);
        
        // Verificar si hemos llegado al arma
        if (Vector3.Distance(transform.position, armaObjectivo.position) < 1f)
        {
            RecogerArma(armaObjectivo.gameObject);
        }
    }
    
    void RecogerArma(GameObject armaDelSuelo)
    {
        // Recoger el arma del suelo
        tengoArma = true;
        municion = 5;
        
        // ¡HACER QUE EL ARMA SEA SIEMPRE VISIBLE DESDE AHORA!
        armaSiempreVisible = true;
        
        // DESACTIVAR el arma del suelo (NO la de la mano)
        armaDelSuelo.SetActive(false);
        
        // ACTIVAR el arma visual en la mano (y mantenerla siempre)
        ConfigurarArmaVisual();
        
        // Resetear búsqueda
        buscandoArma = false;
        armaObjectivo = null;
        
        Debug.Log("¡Arma recogida del suelo! ¡APARECE PARA SIEMPRE en la mano! Munición: " + municion);
    }
   
    // CONDICIONES
    bool EnemigoEnRango()
    {
        if (player == null) return false;
       
        float distancia = Vector3.Distance(transform.position, player.position);
        return distancia <= rangoDeteccion;
    }
   
    bool TengoMunicion()
    {
        return municion > 0;
    }
   
    // ACCIONES
    void Disparar()
    {
        Debug.Log("DISPARAR - Munición: " + municion);
        municion--;
       
        // Mirar hacia el objetivo
        if (player != null)
        {
            Vector3 direccion = (player.position - transform.position).normalized;
            direccion.y = 0; // Solo rotar en Y
            transform.LookAt(transform.position + direccion);
            
            // Crear el rayo láser
            CrearRayoLaser();
            
            // Verificar si el disparo impacta al jugador
            VerificarImpactoDisparo();
        }
        
        // CUANDO SE QUEDA SIN MUNICIÓN, YA NO QUITAMOS EL ARMA
        if (municion <= 0)
        {
            // Ya no tenemos munición pero el arma sigue visible
            Debug.Log("¡Sin munición! Pero el arma permanece visible.");
        }
    }
    
    void CrearRayoLaser()
    {
        if (player == null || lineRenderer == null) return;
        
        // Punto de origen del disparo
        Vector3 puntoOrigen;
        if (puntoDisparo != null)
        {
            puntoOrigen = puntoDisparo.position;
        }
        else
        {
            // Si no hay punto específico, usar la posición del enemigo + offset
            puntoOrigen = transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;
        }
        
        // Punto de destino (jugador)
        Vector3 puntoDestino = player.position + Vector3.up * 0.5f; // Añadir altura para apuntar al centro
        
        // Configurar el LineRenderer
        lineRenderer.SetPosition(0, puntoOrigen);
        lineRenderer.SetPosition(1, puntoDestino);
        lineRenderer.enabled = true;
        
        // Activar el temporizador para ocultar el rayo
        mostrandoRayo = true;
        timerRayo = 0f;
        
        Debug.Log("¡Rayo láser creado!");
    }
    
    void VerificarImpactoDisparo()
    {
        if (player == null) return;
        
        // Crear un Raycast para verificar si el disparo impacta al jugador
        Vector3 puntoOrigen;
        if (puntoDisparo != null)
        {
            puntoOrigen = puntoDisparo.position;
        }
        else
        {
            puntoOrigen = transform.position + transform.forward * 0.5f + Vector3.up * 0.5f;
        }
        
        Vector3 direccion = (player.position - puntoOrigen).normalized;
        RaycastHit hit;
        
        // Hacer el raycast
        if (Physics.Raycast(puntoOrigen, direccion, out hit, rangoDeteccion))
        {
            // Verificar si impactó al jugador
            if (hit.transform == player)
            {
                Debug.Log("¡DISPARO IMPACTÓ AL JUGADOR!");
                player.gameObject.SetActive(false);
            }
        }
    }
   
    void AtacarPunos()
    {
        Debug.Log("ATACAR PUÑOS");
       
        // Moverse hacia el jugador
        if (player != null)
        {
            Vector3 direccion = (player.position - transform.position).normalized;
            transform.position += direccion * 3f * Time.deltaTime;
            direccion.y = 0; // Solo rotar en Y
            transform.LookAt(transform.position + direccion);
        }
    }
   
    void Patrullaje()
    {
        Debug.Log("PATRULLAJE");
       
        // Movimiento de patrullaje simple
        float tiempo = Time.time;
        Vector3 posicion = transform.position;
        posicion.x += Mathf.Sin(tiempo * 0.5f) * 2f * Time.deltaTime;
        posicion.z += Mathf.Cos(tiempo * 0.5f) * 2f * Time.deltaTime;
        transform.position = posicion;
    }
   
    // Visualizar rangos en Scene view
    void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        // Rango de ataque
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        
        // Mostrar línea hacia el arma objetivo
        if (buscandoArma && armaObjectivo != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, armaObjectivo.position);
        }
        
        // Mostrar punto de disparo
        if (puntoDisparo != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(puntoDisparo.position, 0.1f);
        }
    }
}