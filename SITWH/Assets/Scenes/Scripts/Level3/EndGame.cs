using UnityEngine;
using System.Collections;

public class EndGame : MonoBehaviour
{
    bool a = false;

    private void Start()
    {
        StartCoroutine(W());
    }

    void Update()
    {
        if (a && Input.anyKeyDown)
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private IEnumerator W()
    {
        yield return new WaitForSeconds(4f);
        a = true;
    }
}