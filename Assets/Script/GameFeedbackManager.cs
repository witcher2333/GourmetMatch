using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class GameFeedbackManager :
    MonoBehaviour
{
    public static GameFeedbackManager
        Instance
    { get; private set; }

    [Header("Message")]
    [SerializeField]
    private TMP_Text messageText;

    [SerializeField]
    private CanvasGroup messageCanvasGroup;

    [SerializeField]
    private float messageDuration = 1.5f;

    [Header("Tutorial")]
    [SerializeField]
    private GameObject tutorialPanel;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip buttonClip;

    [SerializeField]
    private AudioClip spawnClip;

    [SerializeField]
    private AudioClip mergeClip;

    [SerializeField]
    private AudioClip orderClip;

    [SerializeField]
    private AudioClip coinClip;

    [SerializeField]
    private AudioClip levelUpClip;

    [SerializeField]
    private AudioClip errorClip;

    private Coroutine messageCoroutine;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = 0f;
            messageCanvasGroup.blocksRaycasts =
                false;
        }
    }

    private void Start()
    {
        if (tutorialPanel != null)
        {
            bool tutorialSeen =
                PlayerPrefs.GetInt(
                    "TutorialSeen",
                    0
                ) == 1;

            tutorialPanel.SetActive(
                !tutorialSeen
            );
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void ShowMessage(
        string message,
        bool isError = false
    )
    {
        if (messageText == null ||
            messageCanvasGroup == null)
        {
            return;
        }

        if (messageCoroutine != null)
        {
            StopCoroutine(
                messageCoroutine
            );
        }

        messageText.text = message;

        messageText.color =
            isError
                ? new Color(
                    1f,
                    0.3f,
                    0.3f,
                    1f
                )
                : Color.white;

        messageCoroutine =
            StartCoroutine(
                ShowMessageRoutine()
            );
    }

    private IEnumerator
        ShowMessageRoutine()
    {
        messageCanvasGroup.alpha = 1f;

        yield return
            new WaitForSecondsRealtime(
                messageDuration
            );

        float fadeDuration = 0.25f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            messageCanvasGroup.alpha =
                1f -
                Mathf.Clamp01(
                    elapsed /
                    fadeDuration
                );

            yield return null;
        }

        messageCanvasGroup.alpha = 0f;
        messageCoroutine = null;
    }

    public void CloseTutorial()
    {
        PlayerPrefs.SetInt(
            "TutorialSeen",
            1
        );

        PlayerPrefs.Save();

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(
                false
            );
        }

        PlayButtonSound();
    }

    public void PlayButtonSound()
    {
        PlayClip(buttonClip);
    }

    public void PlaySpawnSound()
    {
        PlayClip(spawnClip);
    }

    public void PlayMergeSound()
    {
        PlayClip(mergeClip);
    }

    public void PlayOrderSound()
    {
        PlayClip(orderClip);
    }

    public void PlayCoinSound()
    {
        PlayClip(coinClip);
    }

    public void PlayLevelUpSound()
    {
        PlayClip(levelUpClip);
    }

    public void PlayErrorSound()
    {
        PlayClip(errorClip);
    }

    private void PlayClip(
        AudioClip clip
    )
    {
        if (audioSource == null ||
            clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            clip
        );
    }
}