using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [Tooltip("Lista de puntos (transforms) que definen el recorrido.")]
    public Transform[] waypoints;

    /// <summary>
    /// Devuelve la posición de un waypoint aleatorio, distinto del índice actual si es posible.
    /// </summary>
    public Vector3 GetRandomWaypointPosition(int currentIndex, out int newIndex)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            newIndex = -1;
            return transform.position; // punto de retorno por defecto
        }

        if (waypoints.Length == 1)
        {
            newIndex = 0;
            return waypoints[0].position;
        }

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, waypoints.Length);
        } while (randomIndex == currentIndex);

        newIndex = randomIndex;
        return waypoints[randomIndex].position;
    }
}