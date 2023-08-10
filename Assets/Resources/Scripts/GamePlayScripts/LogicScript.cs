using Score;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LogicScript : MonoBehaviour
{
    public const float DEAD_ZONE = -30f;

    private GameObject GameOverScreen,
                       ScoreHandler;

    private const float SPAWN_OFFSET_INCREASER = 1.5f,
                        MAX_SCORE_TO_INCREASE_SPAWN_OFFSET = 20f,
                        MAX_SCORE_TO_INCREASE_SPEED = 100f,
                        SHOW_HINT_DELAY_AFTER_START = 2.5f,//in seconds
                        FLASH_HINT_DELAY = 1.5f;           //same

    private bool _hintIsShowing = false,
                 _gameIsPaused = false;

    private FunctionTimer _timer;
    private Text _scoreText;
    private TextMeshProUGUI _textClickToPlay;

    public bool PlayerIsPlaying { get; private set; } = false;
    public GameObject Pipes { get; private set; }
    public GameObject PipeSpawner { get; private set; }
    public Text ScoreText { get => _scoreText; private set => _scoreText = value; }
    public Text HighscoreText { get; private set; }

    public delegate void SpawnDelegate();

    private void Start()
    {
        var spawners = GameObject.FindGameObjectWithTag("Spawners");
        var canvas = GameObject.FindGameObjectWithTag("Canvas");

        Pipes = Resources.Load<GameObject>("Sprites/Pipes");
        PipeSpawner = spawners.transform.Find("PipeSpawner").gameObject;

        ScoreHandler = transform.GetChild(0).gameObject;
        ScoreText = canvas.transform.Find("PlayerScore").GetComponent<Text>();
        HighscoreText = canvas.transform.Find("Highscore").GetComponent<Text>();

        _textClickToPlay = canvas.transform.Find("HintText").GetComponent<TextMeshProUGUI>();
        GameOverScreen = canvas.transform.Find("GameOverScreen").gameObject;

        GameObject.FindGameObjectWithTag("Bird").GetComponent<BirdScript>().Sprites = 
            ChangeSpriteScript.GameSprites.GetRange(0, 3).ToArray();

        //This pice of code is for hat

        /*GameObject.FindWithTag("Hat").transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = 
            ChangeSpriteScript.GameSprites[3];*/
        
        GameObject.FindWithTag("MainCamera").transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = 
            ChangeSpriteScript.GameSprites[4];

        Pipes.transform.Find("TopPipe").GetComponent<SpriteRenderer>().sprite = 
            ChangeSpriteScript.GameSprites[5];
        Pipes.transform.Find("BottomPipe").GetComponent<SpriteRenderer>().sprite = 
            ChangeSpriteScript.GameSprites[5];

    }
    private void CheckPause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = _gameIsPaused ? 1 : 0;
            _gameIsPaused = !_gameIsPaused;
        }
    }

    private void Update()
    {
        CheckPause();

        if (!PlayerIsPlaying)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButton((short)MouseButton.Left))
            {
                PipeSpawner.SetActive(true);
                PlayerIsPlaying = true;
                _hintIsShowing = false;
                PipeSpawner.GetComponent<PipeSpawnerScript>().SpawnPipes();
                return;
            }
            if (!_hintIsShowing)
            {
                FunctionTimer.StartAndUpdateTimer
                    (ref _timer, SHOW_HINT_DELAY_AFTER_START, SetHintIsShowingToTrue);
                if (_hintIsShowing)
                    _timer = null;
            }
        }

        if (_hintIsShowing) FlashHint();
        else _textClickToPlay.enabled = false;
    }

    private void SetHintIsShowingToTrue()
    {
        _hintIsShowing = true;
    }

    private void FlashHint()
    {
        if (!PlayerIsPlaying && _textClickToPlay != null)
            FunctionTimer.StartAndUpdateTimer(ref _timer, FLASH_HINT_DELAY, SwitchTextCondition);
        else
            _hintIsShowing = false;
    }

    private void SwitchTextCondition()
    {
        _textClickToPlay.enabled = !_textClickToPlay.enabled;
    }

    public void OnScoreIncreased()
    {
        var scoreHandler = ScoreHandler.GetComponent<ScoreHandlerScript>();

        scoreHandler.AddScore();
        if (scoreHandler.PlayerScore <= MAX_SCORE_TO_INCREASE_SPEED)
            Pipes.GetComponent<PipeMoveScript>().IncreaseSpeed();
    }

    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Pipes.GetComponent<PipeMoveScript>().ResetSpeed();
    }

    public void OnMainMenuButtonClick()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    [ContextMenu("Game Over")]
    public void GameOver()
    {
        if (!GameOverScreen.IsUnityNull())
        {
            if (!GameOverScreen.activeInHierarchy)
                HighscoreManager.AddHighscoreEntry(ScoreHandler.GetComponent<ScoreHandlerScript>().PlayerScore);
            GameOverScreen.SetActive(true);
        }
    }

    /// <summary>
    /// Returns the lowest, closest and the highest, farthest points for spawning depends on the player csore
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="spawnOffset"></param>
    /// <returns></returns>
    public float[] DefineSpawnRange(float pos, float spawnOffset, bool pipeIsCalling = false)
    {
        float[] range = new float[2];
        if (pipeIsCalling)
        {
            var playerScore = ScoreHandler.GetComponent<ScoreHandlerScript>().PlayerScore;

            spawnOffset = playerScore < MAX_SCORE_TO_INCREASE_SPAWN_OFFSET ?
                spawnOffset += SPAWN_OFFSET_INCREASER * (playerScore / MAX_SCORE_TO_INCREASE_SPAWN_OFFSET) :
                spawnOffset += SPAWN_OFFSET_INCREASER;
        }
        range[0] = pos - spawnOffset;
        range[1] = pos + spawnOffset;

        return range;
    }
}
