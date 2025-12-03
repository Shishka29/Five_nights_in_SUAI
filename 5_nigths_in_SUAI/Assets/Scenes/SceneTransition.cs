using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [Header("UI для затемнения")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    private void OnEnable()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// Запуск затемнения и перехода на сцену
    /// </summary>
    public void FadeToScene(string sceneName)
    {
        if (fadeImage != null)
            fadeImage.gameObject.SetActive(true);

        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        if (fadeImage == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        float timer = 0f;
        Color color = fadeImage.color;
        color.a = 0f;
        fadeImage.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
