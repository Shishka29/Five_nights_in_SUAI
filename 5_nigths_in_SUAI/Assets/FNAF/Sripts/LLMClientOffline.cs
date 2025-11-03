using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using TMPro;

public class LLMClientOffline : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI outputText;

    [Header("LLaMA CLI Settings")]
    public string llamaFolder = "LLM/Models"; // Папка внутри Assets
    public string exeName = "llama-cli.exe";
    public string modelName = "ggml-model-q4_0.gguf";

    [Header("Generation Settings")]
    public int nPredict = 100;
    [Tooltip("Минимальный интервал обновления фраз (сек)")]
    public float minUpdateTime = 5f;
    [Tooltip("Максимальный интервал обновления фраз (сек)")]
    public float maxUpdateTime = 10f;

    private string llamaExePath => Path.Combine(Application.dataPath, llamaFolder, exeName);
    private string modelPath => Path.Combine(Application.dataPath, llamaFolder, modelName);

    private Coroutine generationCoroutine;

    public void StartGenerating(string prompt)
    {
        if (generationCoroutine != null)
            StopCoroutine(generationCoroutine);

        generationCoroutine = StartCoroutine(RunLlamaCliCoroutine(prompt));
    }

    private IEnumerator RunLlamaCliCoroutine(string prompt)
    {
        if (!File.Exists(llamaExePath) || !File.Exists(modelPath))
        {
            UnityEngine.Debug.LogError("❌ LLaMA CLI или модель не найдены!");
            if (outputText != null) outputText.text = "Ошибка: файлы не найдены";
            yield break;
        }

        while (true)
        {
            string result = "";

            var psi = new ProcessStartInfo
            {
                FileName = llamaExePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = psi })
            {
                process.Start();

                // Записываем модель и prompt через stdin
                process.StandardInput.WriteLine($"--model \"{modelPath}\"");
                process.StandardInput.WriteLine($"--n_predict {nPredict}");
                process.StandardInput.WriteLine(prompt);
                process.StandardInput.Flush();
                process.StandardInput.Close();

                while (!process.HasExited)
                {
                    while (!process.StandardOutput.EndOfStream)
                    {
                        string line = process.StandardOutput.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            result += line + "\n";
                            if (outputText != null)
                                outputText.text = result;
                        }
                    }

                    while (!process.StandardError.EndOfStream)
                    {
                        string err = process.StandardError.ReadLine();
                        if (!string.IsNullOrEmpty(err))
                            UnityEngine.Debug.LogError("LLaMA CLI Error: " + err);
                    }

                    yield return null;
                }
            }

            UnityEngine.Debug.Log("✅ Ответ LLaMA:\n" + result);

            // Ждём рандомный интервал перед следующим запросом
            float waitTime = Random.Range(minUpdateTime, maxUpdateTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
