using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class LLMTest : MonoBehaviour
{
    public string llamaPath;   // полный путь к llama-cli.exe
    public string modelPath;   // полный путь к модели .gguf

    void Start()
    {
        StartCoroutine(TestLLM());
    }

    private IEnumerator TestLLM()
    {
        var psi = new ProcessStartInfo
        {
            FileName = llamaPath,
            Arguments = $"--model \"{modelPath}\" --prompt \"ѕривет\" --n_predict 10",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = psi })
        {
            process.Start();

            while (!process.HasExited)
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    string line = process.StandardOutput.ReadLine();
                    UnityEngine.Debug.Log("LLM: " + line);
                }

                while (!process.StandardError.EndOfStream)
                {
                    string err = process.StandardError.ReadLine();
                    UnityEngine.Debug.LogError("LLM ERROR: " + err);
                }

                yield return null;
            }
        }

        UnityEngine.Debug.Log("LLM test finished");
    }
}
