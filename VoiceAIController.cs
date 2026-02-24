using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;

public class VoiceAIController : MonoBehaviour
{
    [Header("API Settings")]
    public string apiKey = "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"; // Рекомендую удалить ключ из кода после теста!

    [Header("References")]
    public AudioSource audioSource;
    public Animator animator;
    
    [Header("UI WinCanvas Settings")]
    public GameObject successUIPanel; 
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI recommendationsText;
    public TextMeshProUGUI starsText;

    [Header("State")]
    public bool isRoomActive = false; 

    [Header("VR Input")]
    public XRNode inputSource = XRNode.LeftHand; 
    private AudioClip _recording;
    private bool _isRecording = false;
    private string _micName;
    private int _micFreq;
    private List<JObject> _chatHistory = new List<JObject>();

    void Start()
    {
        if (successUIPanel != null) successUIPanel.SetActive(false);
        

        if (Microphone.devices.Length > 0)
        {
            _micName = Microphone.devices[0];
            int min, max;
            Microphone.GetDeviceCaps(_micName, out min, out max);
            _micFreq = (max > 0) ? max : 44100;
        }

       
        _chatHistory.Add(new JObject { 
            ["role"] = "system", 
            ["content"] = "Ты — пациент на приеме у психолога. Отвечай кратко, проявляй эмоции. " +
                          "Если психолог тебе действительно помог или сказал что-то очень мудрое, " +
                          "начни свой ответ строго с фразы [FINISH], а затем поблагодари." 
        });
    }


    public void ActivateRoom() 
    {
        isRoomActive = true;
        Debug.Log("<color=yellow>[СИСТЕМА]: Психолог готов к работе. Можно нажимать триггер!</color>");
    }

    void Update()
    {
        
        if (!isRoomActive) return;

        bool isPressed = CheckInput();


        if (isPressed && !_isRecording) 
        {
            StartRecording();
        }
        else if (!isPressed && _isRecording) 
        {
            StartCoroutine(StopAndProcessRoutine());
        }


        if (animator != null) 
        {
            animator.SetBool("isTalking", audioSource.isPlaying);
        }
    }

    bool CheckInput()
    {
        
        if (Input.GetKey(KeyCode.O)) return true;


        InputDevice device = InputDevices.GetDeviceAtXRNode(inputSource);
        if (device.isValid && device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed)) 
        {
            return pressed;
        }
        return false;
    }

    void StartRecording()
    {
        _isRecording = true;
        _recording = Microphone.Start(_micName, false, 10, _micFreq);
        Debug.Log("<color=red>● ПАЦИЕНТ СЛУШАЕТ...</color>");
    }

    IEnumerator StopAndProcessRoutine()
    {
        _isRecording = false;
        yield return new WaitForSeconds(0.1f); 
        int lastSample = Microphone.GetPosition(_micName);
        Microphone.End(_micName);
        
        if (lastSample < 1000) yield break;

        Debug.Log("<color=white>⌛ ПАЦИЕНТ ОБДУМЫВАЕТ ВАШИ СЛОВА...</color>");
        byte[] wavData = WavUtility.FromAudioClip(TrimAudio(_recording, lastSample));
        if (wavData != null) StartCoroutine(SendToWhisper(wavData));
    }

    IEnumerator SendToWhisper(byte[] audioData)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        using (UnityWebRequest request = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string text = JObject.Parse(request.downloadHandler.text)["text"].ToString();
                StartCoroutine(AskGPT(text));
            }
        }
    }

    IEnumerator AskGPT(string userText)
    {
        _chatHistory.Add(new JObject { ["role"] = "user", ["content"] = userText });
        JObject body = new JObject { ["model"] = "gpt-4o-mini", ["messages"] = JArray.FromObject(_chatHistory) };

        using (UnityWebRequest request = CreatePostRequest("https://api.openai.com/v1/chat/completions", body.ToString()))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string aiText = JObject.Parse(request.downloadHandler.text)["choices"][0]["message"]["content"].ToString();
                

                if (aiText.Contains("[FINISH]"))
                {
                    aiText = aiText.Replace("[FINISH]", "");
                    StartCoroutine(GetFinalEvaluation());
                }

                _chatHistory.Add(new JObject { ["role"] = "assistant", ["content"] = aiText });
                StartCoroutine(GenerateVoice(aiText));
            }
        }
    }

    IEnumerator GetFinalEvaluation()
    {

        string evalPrompt = "Сделай краткий отчет: 2 совета психологу и оценка от 1 до 5.";
        _chatHistory.Add(new JObject { ["role"] = "user", ["content"] = evalPrompt });
        
        JObject body = new JObject { ["model"] = "gpt-4o-mini", ["messages"] = JArray.FromObject(_chatHistory) };

        using (UnityWebRequest request = CreatePostRequest("https://api.openai.com/v1/chat/completions", body.ToString()))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string report = JObject.Parse(request.downloadHandler.text)["choices"][0]["message"]["content"].ToString();
                ShowResultUI(report);
            }
        }
    }

    void ShowResultUI(string report)
    {
        if (successUIPanel != null)
        {
            successUIPanel.SetActive(true);
            headerText.text = "СЕАНС ЗАВЕРШЕН";
            recommendationsText.text = report;

            int score = 5; 
            for (int i = 1; i <= 5; i++) if (report.Contains(i.ToString())) score = i;
            
            string smiles = "";
            for (int i = 0; i < score; i++) smiles += "★ "; 
            starsText.text = "РЕЗУЛЬТАТ: " + smiles;
        }
    }

    IEnumerator GenerateVoice(string text)
    {
        string json = "{\"model\":\"tts-1\",\"input\":\"" + text + "\",\"voice\":\"nova\"}";
        using (UnityWebRequest request = CreatePostRequest("https://api.openai.com/v1/audio/speech", json))
        {
            var handler = new DownloadHandlerAudioClip(request.url, AudioType.MPEG);
            request.downloadHandler = handler;
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                audioSource.clip = handler.audioClip;
                audioSource.Play();
            }
        }
    }

    UnityWebRequest CreatePostRequest(string url, string json)
    {
        UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + apiKey);
        return req;
    }

    AudioClip TrimAudio(AudioClip clip, int samples)
    {
        if (samples <= 0) return null;
        AudioClip t = AudioClip.Create("trimmed", samples, clip.channels, clip.frequency, false);
        float[] d = new float[samples * clip.channels];
        clip.GetData(d, 0);
        t.SetData(d, 0);
        return t;
    }

}
