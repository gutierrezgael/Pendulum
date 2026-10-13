using UnityEngine;
using UnityEngine.SceneManagement;
using ColliderEventSystem;
public class GameOverHandler : MonoBehaviour
{
    [Tooltip("Name of the game over scene (must be in File > Build Profiles > Scene List)")]
    public string loseScene = "Pendulum-Lose";

    [Tooltip("Seconds to wait before loading the scene (time to hear a sound, see an animation...)")]
    public float delay = 0.5f;

    [Header("CES link (optional)")]
    [Tooltip("The same variable as in the player's Health component. Reset so the next run doesn't start at 0 HP.")]
    public IntVariable healthVariable;
    public int healthAfterReset = 100;

    bool isOver;

    public void ShowGameOver()
    {
        if (isOver) return;
        isOver = true;

        // Stop shooting while we wait
        PlayerShooter shooter = FindAnyObjectByType<PlayerShooter>();
        if (shooter != null) shooter.enabled = false;

        Invoke(nameof(LoadLoseScene), delay);
    }

    void LoadLoseScene()
    {
        Time.timeScale = 1f;

        if (healthVariable != null) healthVariable.RuntimeValue = healthAfterReset;

        // Fresh player on the next run
        if (PersistentPlayer.Instance != null)
            Destroy(PersistentPlayer.Instance.gameObject);

        SceneManager.LoadScene(loseScene);
    }
}