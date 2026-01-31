using UnityEngine;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public void OnClickPlay()
    {
        SceneManager.LoadScene("Main");
    }
}
