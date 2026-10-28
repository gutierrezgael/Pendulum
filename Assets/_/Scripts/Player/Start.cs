using UnityEngine;
using UnityEngine.SceneManagement;

public class MonScript : MonoBehaviour
{
    public string nomDeLaScene = "Pendulum-Floor_3"; 

    public void ChangeLaScene()
    {
        SceneManager.LoadScene(nomDeLaScene);
    }
}