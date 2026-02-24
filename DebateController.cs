using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class DebateController : MonoBehaviour
{
    public string apiKey = "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";
    public string roleName; 
    [TextArea] public string roleInstructions;
    public DebateController opponent;
    public AudioSource audioSource;
    public Animator animator;

    [HideInInspector] public int argumentsSaid = 0;
    private const int MaxArguments = 2;

    private List<JObject> _chatHistory = new List<JObject>();

    void Start()
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        _chatHistory.Add(new JObject { ["role"] = "system", ["content"] = $"Ты {roleName}. {roleInstructions}. Отвечай кратко (1-2 предложения)." });
    }

    public void StartDebate()
    {
        argumentsSaid = 0;
        if (opponent != null) opponent.argumentsSaid = 0;
        StartCoroutine(AskGPT("Начни дебаты и представь свою позицию."));
    }

    public IEnumerator AskGPT(string lastMsg)
    {
        if (argumentsSaid >= MaxArguments) yield break;

        argumentsSaid++;
        _chatHistory.Add(new JObject { ["role"] = "user", ["content"] = lastMsg });
        JObject body = new JObject { ["model"] = "gpt-4o-mini", ["messages"] = JArray.FromObject(_chatHistory) };

        using (UnityWebRequest request = CreateRequest("https://api.openai.com/v1/chat/completions", body.ToString()))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string text = JObject.Parse(request.downloadHandler.text)["choices"][0]["message"]["content"].ToString();
                _chatHistory.Add(new JObject { ["role"] = "assistant", ["content"] = text });
                
                yield return StartCoroutine(PlayVoice(text));


                if (opponent != null && opponent.argumentsSaid < MaxArguments)
                {
                    StartCoroutine(opponent.AskGPT(text));
                }
                else if (argumentsSaid >= MaxArguments && (opponent == null || opponent.argumentsSaid >= MaxArguments))
                {
                    Debug.Log("<color=magenta>[СУД]: Аргументы закончились. Судья, выносите вердикт!</color>");
                }
            }
        }
    }

    IEnumerator PlayVoice(string text)
    {
        if (animator != null) animator.SetBool("isTalking", true);
        string voice = (roleName == "Парламент") ? "onyx" : "alloy";
        string json = "{\"model\":\"tts-1\",\"input\":\"" + text + "\",\"voice\":\"" + voice + "\"}";
        using (UnityWebRequest request = CreateRequest("https://api.openai.com/v1/audio/speech", json))
        {
            var handler = new DownloadHandlerAudioClip(request.url, AudioType.MPEG);
            request.downloadHandler = handler;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                audioSource.clip = handler.audioClip;
                audioSource.Play();
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
        }
        if (animator != null) animator.SetBool("isTalking", false);
    }

    UnityWebRequest CreateRequest(string url, string json)
    {
        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);
        return req;
    }

}
