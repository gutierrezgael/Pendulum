using UnityEngine;
using UnityEngine.SceneManagement;

public class MonScript3 : MonoBehaviour
{
    public string nomDeLaScene = "Pendulum-Shop"; 

    public void ChangeLaScene()
    {
        SceneManager.LoadScene(nomDeLaScene);
    }
}