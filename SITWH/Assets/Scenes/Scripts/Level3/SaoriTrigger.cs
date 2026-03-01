using UnityEngine;

public class SaoriTrigger : MonoBehaviour
{
    [Header("Referencia a la vida")]
    public CrumpsLogic crumpsLogic;

    [Header("Movimiento al morir")]
    public float floatSpeed = 0.03f;
    public float rotationSpeed = 30f;

    [Header("Altura máxima")]
    public float maxHeight = 0.217f; // Pon aquí el valor Y límite

    private bool isDead = false;

    void Update()
    {
        if (crumpsLogic != null && crumpsLogic.health <= 0 && !isDead)
        {
            isDead = true;
            transform.Rotate(90f,0,0);
        }

        if (isDead)
        {
            
            if (transform.position.y < maxHeight)
            {
                transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);
            }

            // Sigue rotando siempre
            transform.Rotate(0f, rotationSpeed * Time.deltaTime,0f );
        }
    }
}