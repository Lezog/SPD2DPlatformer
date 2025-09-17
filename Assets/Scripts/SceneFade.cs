using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFade : MonoBehaviour
{
    public static SceneFade Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    private IEnumerator FadeIn()
    {
        Color c = fadeImage.color;
        for (float t = 1f; t >= 0f; t -= Time.deltaTime / fadeTime)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }
    }

    private IEnumerator FadeOut(string sceneName)
    {
        Color c = fadeImage.color;
        for (float t = 0f; t <= 1f; t += Time.deltaTime / fadeTime)
        {
            c.a = t;
            fadeImage.color = c;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
        StartCoroutine(FadeIn());
    }
}