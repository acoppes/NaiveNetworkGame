using UnityEngine;
using UnityEngine.SceneManagement;

namespace Server
{
    public class ReloadSceneController : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadScene("ServerScene");
        }
    }
}
