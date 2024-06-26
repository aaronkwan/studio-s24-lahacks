using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    #region Singleton
    
    public static Manager Instance { get; private set; }

    public View m_view;
    public Sound m_sound;
    public Display m_display;
    public Player m_player;
    public Controller m_controller;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Pause
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        m_display.DisplayPause(isPaused);
    }
    #endregion

    #region State

    public int timer = 1;
    public float life = 30f;
    public int total_sticks = 0;
    private Coroutine timerCoroutine;
    public bool isPaused = false;
    public bool isCheating = false;
    IEnumerator Timer()
    {
        int i = 0;
        while (true)
        {
            if (!isPaused)
            {
                if (i == 50)
                {
                    timer++;
                    i = 0;
                }
                life -= 0.02f;
                i++;
            }
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void StartGame(bool isMobile)
    {
        m_display.StartGame(isMobile);
        caveCover.gameObject.SetActive(true);
        timerCoroutine = StartCoroutine(Timer());
        SpawnNewStick();
    }
    public void EndGame(bool won)
    {
        StopCoroutine(timerCoroutine);
        m_display.DisplayGameOver(won);
        isPaused = true;
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }




    #endregion

    #region Transition

    [SerializeField] private SpriteRenderer caveCover;
    private Coroutine fadeCoroutine;
    private WaitForSeconds delay = new WaitForSeconds(0.02f);
    public void EnterCave()
    {
        m_sound.EnterCave();
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeCover(0.3f, 25));
    }
    public void ExitCave()
    {
        m_sound.ExitCave();
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeCover(1f, 10));
    }
    private IEnumerator FadeCover(float targetAlpha, int ticks)
    {
        float startAlpha = caveCover.color.a;
        float alphaStep = (targetAlpha - startAlpha) / ticks;
        for (int i = 0; i < ticks; i++)
        {
            float newAlpha = startAlpha + alphaStep * i;
            caveCover.color = new Color(caveCover.color.r, caveCover.color.g, caveCover.color.b, newAlpha);
            yield return delay;
        }
        caveCover.color = new Color(caveCover.color.r, caveCover.color.g, caveCover.color.b, targetAlpha);
    }
    #endregion

    #region Footstep

    public enum GROUND
    {
        GRASS, GRAVEL, STONE
    }

    public GROUND currentGround;
    public float currentSpeed;

    #endregion

    #region Stick
    [SerializeField] private GameObject stick;
    [SerializeField] private Transform field;
    public bool showSticks = false;
    public void SpawnNewStick()
    {
        float randomX = Random.Range(-2, 32);
        float randomY = Random.Range(-12, 12);
        Vector3 randomPosition = new Vector3(randomX, randomY, stick.transform.position.z);
        GameObject stickCopy = Instantiate(stick, randomPosition, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
        stickCopy.transform.SetParent(field);
    }
    #endregion

}
