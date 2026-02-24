using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class StudentController : MonoBehaviour
{
    [Header("OpenAI API")]
    public string apiKey = "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";

    [Header("Ссылки")]
    public AudioSource audioSource;
    public Animator animator;
    public LessonManager lessonManager; 
    [Header("Настройки VR")]
    public XRNode inputSource = XRNode.RightHand; 
    
    [Header("Параметры Ученика")]
    public string lessonTopic = "Фотосинтез";
    
    private AudioClip _recording;
    private bool _isRecording = false;
    private bool _isThinking = false;
    private string _micName;
    private List<JObject> _chatHistory = new List<JObject>();

    void Start()
    {
        
        if (Microphone.devices.Length > 0) _micName = Microphone.devices[0];

        
        _chatHistory.Add(new JObject { 
            ["role"] = "system", 
            ["content"] = $"Ты ученик на уроке. Тема: {lessonTopic}. Веди себя как ребенок. Если учитель объясняет сложно — переспрашивай. Если всё понятно, скажи [ПОНЯЛ]." 
        });
        
        
        System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
    }

    void Update()
    {
        
        if (lessonManager == null || !lessonManager.isLessonStarted) return;

        
        bool isPressed = CheckInput();

        if (isPressed && !_isRecording && !_isThinking)
        {
            StartRecording();
        }
        else if (!isPressed && _isRecording)
        {
            StopAndProcess();
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
        _recording = Microphone.Start(_micName, false, 10, 44100);
        Debug.Log("<color=red>● ЗАПИСЬ (ПРАВЫЙ ТРИГГЕР)...</color>");
    }

    void StopAndProcess()
    {
        _isRecording = false;
        int pos = Microphone.GetPosition(_micName);
        Microphone.End(_micName);
        if (pos > 1000) StartCoroutine(ProcessVoice(pos));
    }

    IEnumerator ProcessVoice(int samples)
    {
        _isThinking = true;
        Debug.Log("Распознаю речь учителя...");

        
        byte[] wavData = WavUtility.FromAudioClip(_recording);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "speech.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        using (UnityWebRequest request = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string text = JObject.Parse(request.downloadHandler.text)["text"].ToString();
                Debug.Log($"<color=yellow>Вы сказали:</color> {text}");
                yield return StartCoroutine(AskGPT(text));
            }
        }
        _isThinking = false;
    }

    IEnumerator AskGPT(string userText)
    {
        _chatHistory.Add(new JObject { ["role"] = "user", ["content"] = userText });

        JObject body = new JObject {
            ["model"] = "gpt-4o-mini",
            ["messages"] = JArray.FromObject(_chatHistory)
        };

        using (UnityWebRequest request = CreatePostRequest("https://api.openai.com/v1/chat/completions", body.ToString()))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string aiText = JObject.Parse(request.downloadHandler.text)["choices"][0]["message"]["content"].ToString();
                
                if (aiText.Contains("[ПОНЯЛ]"))
                {
                    aiText = aiText.Replace("[ПОНЯЛ]", "О! Теперь мне всё ясно, спасибо за урок!");
                    Debug.Log("<color=green>Цель достигнута: Ученик всё понял!</color>");
                }

                _chatHistory.Add(new JObject { ["role"] = "assistant", ["content"] = aiText });
                yield return StartCoroutine(Speak(aiText));
            }
        }
    }

    IEnumerator Speak(string text)
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

    
    public void PlayInitialAnimation()
    {
        Debug.Log("<color=cyan>[УЧЕНИК]: О, урок начался! Я готов слушать.</color>");
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

}
