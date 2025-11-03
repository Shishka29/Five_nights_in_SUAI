using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

public class SimpleTokenizer
{
    private Dictionary<string, int> _vocab;
    private Dictionary<int, string> _invVocab;

    public SimpleTokenizer(string vocabFilePath)
    {
        if (!File.Exists(vocabFilePath))
            throw new Exception("Файл токенизатора не найден: " + vocabFilePath);

        string json = File.ReadAllText(vocabFilePath);
        _vocab = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        _invVocab = _vocab.ToDictionary(k => k.Value, v => v.Key);
    }

    // Токенизация текста
    public int[] Encode(string text)
    {
        var tokens = new List<int>();
        foreach (char c in text)
        {
            string s = c.ToString();
            if (_vocab.ContainsKey(s))
                tokens.Add(_vocab[s]);
            else
                tokens.Add(_vocab.ContainsKey("<unk>") ? _vocab["<unk>"] : 0);
        }
        return tokens.ToArray();
    }

    // Декодирование ID обратно в текст
    public string Decode(int[] tokens)
    {
        var chars = new List<string>();
        foreach (int t in tokens)
        {
            if (_invVocab.ContainsKey(t))
                chars.Add(_invVocab[t]);
            else
                chars.Add("?");
        }
        return string.Join("", chars);
    }
}
