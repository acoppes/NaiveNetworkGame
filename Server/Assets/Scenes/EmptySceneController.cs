using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes
{
    public class EmptySceneController : MonoBehaviour
    {
        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                SceneManager.LoadScene("ServerScene");
            }
        }
    }
}
