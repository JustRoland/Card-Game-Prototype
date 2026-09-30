using UnityEngine;
using UnityEngine.SceneManagement;


namespace Utility
{
    public class SceneChanger : MonoBehaviour
    {
        public void LoadScene(Scene scene)
        {
            SceneManager.LoadScene(scene.buildIndex);
        }

        public void LoadScene(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
