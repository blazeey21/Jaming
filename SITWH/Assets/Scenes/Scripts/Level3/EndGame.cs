using UnityEngine;

public class EndGame : MonoBehaviour
{
    void Update()
    {
        // Si se presiona cualquier tecla
        if (Input.anyKeyDown)
        {
            Application.Quit();

#if UNITY_EDITOR
            // Esto permite que funcione también dentro del Editor
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}