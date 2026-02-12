using UnityEngine;


public class ParitculasDesativar : MonoBehaviour
{
    [Tooltip("Objeto a vigilar. Si es destruido o desactivado, este objeto se desactivará.")]
    public GameObject objetoAVigilar;

    void Start()
    {
        if (objetoAVigilar == null || !objetoAVigilar.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (objetoAVigilar == null || !objetoAVigilar.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
    }
}