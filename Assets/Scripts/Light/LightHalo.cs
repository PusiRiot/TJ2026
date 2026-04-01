using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LightHalo : MonoBehaviour
{
    [Header("Configuración de la Linterna")]
    public float distanciaMaxima = 10f; // Distancia máxima en el aire
    public float anguloVision = 45f;    // Debe coincidir con el ángulo de tu Spotlight
    public int segmentos = 30;          // Cuán redondo será el círculo (30 es un buen equilibrio)

    [Header("Físicas")]
    public LayerMask capaObstaculos;

    private Mesh mallaCirculo;

    void Start()
    {
        mallaCirculo = new Mesh();
        mallaCirculo.name = "Disco Linterna";
        GetComponent<MeshFilter>().mesh = mallaCirculo;
    }

    void LateUpdate()
    {
        DibujarDisco();
    }

    void DibujarDisco()
    {
        // 1. Calculamos la distancia real: ¿Chocamos con algo o llegamos al límite en el aire?
        float distanciaActual = distanciaMaxima;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit impacto, distanciaMaxima, capaObstaculos))
        {
            distanciaActual = impacto.distance;
        }

        // 2. Calculamos el radio del círculo a esa distancia usando trigonometría
        // Mathf.Tan requiere el ángulo en radianes y usamos la mitad del ángulo total.
        float radio = distanciaActual * Mathf.Tan(anguloVision * 0.5f * Mathf.Deg2Rad);

        // 3. Preparamos los datos para la malla
        Vector3[] vertices = new Vector3[segmentos + 2];
        int[] triangulos = new int[segmentos * 3];

        // El primer vértice es el centro del disco
        // Lo posicionamos hacia adelante (eje Z local) a la distancia calculada
        vertices[0] = new Vector3(0, 0, distanciaActual);

        // Calculamos cuántos grados avanzamos por cada segmento del círculo
        float anguloPaso = 360f / segmentos;

        for (int i = 0; i <= segmentos; i++)
        {
            // Convertimos el ángulo a radianes para las funciones matemáticas
            float angulo = i * anguloPaso * Mathf.Deg2Rad;

            // Calculamos X e Y para hacer un círculo vertical (Z es la profundidad)
            float x = Mathf.Sin(angulo) * radio;
            float y = Mathf.Cos(angulo) * radio;

            // Asignamos el vértice en el borde del círculo
            vertices[i + 1] = new Vector3(x, y, distanciaActual);

            // Conectamos los puntos para formar los triángulos de la malla
            if (i < segmentos)
            {
                triangulos[i * 3] = 0;               // Centro
                triangulos[i * 3 + 1] = i + 1;       // Punto actual en el borde
                triangulos[i * 3 + 2] = i + 2;       // Siguiente punto en el borde
            }
        }

        // 4. Actualizamos la malla para que Unity la dibuje
        mallaCirculo.Clear();
        mallaCirculo.vertices = vertices;
        mallaCirculo.triangles = triangulos;
        mallaCirculo.RecalculateNormals();
    }
}