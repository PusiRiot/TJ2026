using System.ComponentModel;
using UnityEngine;

// Se ejecuta tanto en edit como en play mode
[ExecuteAlways]
public class GlobalLightManager : MonoBehaviour
{
    [Header("Configuración Global de Luces URP\n" +
            " - Use this function \"1 - (distance/MAXDISTANCE)^fall-offExponent\" in graphtoy to check the graph\n" +
            "in graphtoy to check the graph")]
    [Tooltip("Controla la dureza de la curva de caída de todas las luces.")]
    [Description()]
    [Range(0f, 100f)]
    public float fallOffExponent = 20f;

    void Update()
    {
        // Enviamos el valor del inspector a todos los shaders del juego
        // Usamos un nombre específico que luego leeremos en el HLSL
        Shader.SetGlobalFloat("_GlobalFalloffExponent", fallOffExponent);
    }
}