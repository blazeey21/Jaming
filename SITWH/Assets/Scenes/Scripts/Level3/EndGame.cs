using UnityEngine;

public class EndGame : MonoBehaviour
{
    void Update()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            // Solo teclas del teclado (evitamos mouse)
            if (key.ToString().StartsWith("Mouse"))
                continue;

            if (Input.GetKeyDown(key))
            {
                Application.Quit();

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}