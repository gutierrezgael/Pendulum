using UnityEngine;
using UnityEngine.SceneManagement;

public class MonScript2 : MonoBehaviour
{
    public string nomDeLaScene = "Pendulum-Menu"; 

    public void ChangeLaScene()
    {
        SceneManager.LoadScene(nomDeLaScene);
    }
}