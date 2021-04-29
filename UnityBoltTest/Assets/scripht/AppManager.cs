using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : MonoBehaviour
{
    [SerializeField]
    private HeadlessServerManager1 _headlessServerManager = null;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //Le client va dans la scene menu
        if (!_headlessServerManager.IsServer)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
