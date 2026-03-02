using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class EndGame : MonoBehaviour
{
    bool a=false;

    private void Start()
    {
        W();
    }
    void Update()
    {
        if (a)
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
    private IEnumerator W()
    {
        yield return new WaitForSeconds(4);
        a = true;
       
    }
}