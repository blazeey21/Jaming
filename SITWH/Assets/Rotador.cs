using UnityEngine;

public class Rotador : MonoBehaviour
{
    // Velocidades de rotación en grados por segundo para cada eje
    public float velocidadX = 30f;
    public float velocidadY = 45f;
    public float velocidadZ = 60f;

    // Update se llama una vez por frame
    void Update()
    {
        // Rotar alrededor del eje X (rojo)
        transform.Rotate(Vector3.right * velocidadX * Time.deltaTime);
        // Rotar alrededor del eje Y (verde)
        transform.Rotate(Vector3.up * velocidadY * Time.deltaTime);
        // Rotar alrededor del eje Z (azul)
        transform.Rotate(Vector3.forward * velocidadZ * Time.deltaTime);
    }
}