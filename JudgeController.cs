using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class JudgeController : MonoBehaviour
{
    public string apiKey = "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";
    
    [Header("Связи")]
    public DebateController npc1; 
    public DebateController npc2; 
    public JudgeRoomManager roomManager; 
    
    [Header("UI")]
    public GameObject winCanvas;
    public TextMeshProUGUI verdictText;

    private bool _isRecording = false;
    private AudioClip _recording;
    private string _micName;

    void Start()
    {
        if (winCanvas != null) winCanvas.SetActive(false);
        if (Microphone.devices.Length > 0) _micName = Microphone.devices[0];
        else Debug.LogError("Микрофон не найден!");
    }

    void Update()
    {
        // 1. Проверяем, наступило ли время судьи
        if (roomManager == null || !roomManager.isSessionActive) return;
        if (npc1.argumentsSaid < 2 || npc2.argumentsSaid < 2) return;

        // 2. Получаем правый контроллер
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        
        // Пытаемся считать состояние триггера (как float от 0 до 1, это надежнее кнопки)
        bool triggerPressed = false;
        if (rightHand.isValid)
        {
            if (rightHand.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                triggerPressed = triggerValue > 0.1f; // Считаем нажатым, если прожали больше чем на 10%
            }
        }

        // 3. Логика записи (Триггер или клавиша V)
        if ((triggerPressed || Input.GetKeyDown(KeyCode.V)) && !_isRecording) 
        {
            StartRecording();
        }
        else if ((!triggerPressed && !Input.GetKey(KeyCode.V)) && _isRecording) 
        {
            StartCoroutine(ProcessVerdict());
        }
    }

    void StartRecording()
    {
        _isRecording = true;
        _recording = Microphone.Start(_micName, false, 10, 44100);
        Debug.Log("<color=magenta>[СУДЬЯ]: Запись вердикта пошла...</color>");
    }

    IEnumerator ProcessVerdict()
    {
        _isRecording = false;
        int pos = Microphone.GetPosition(_micName);
        Microphone.End(_micName);
        
        if (pos < 1000) 
        {
            Debug.LogWarning("Запись слишком короткая.");
            yield break;
        }

        Debug.Log("<color=yellow>[СУДЬЯ]: Обработка голоса...</color>");
        
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", WavUtility.FromAudioClip(_recording), "v.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        using (UnityWebRequest request = UnityWebRequest.Post("https://api.openai.com/v1/audio/transcriptions", form))
        {
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string text = JObject.Parse(request.downloadHandler.text)["text"].ToString();
                winCanvas.SetActive(true);
                verdictText.text = "ВЕРДИКТ: " + text;
                Debug.Log("<color=green>Успех!</color>");
            }
            else
            {
                Debug.LogError("Ошибка Whisper: " + request.downloadHandler.text);
            }
        }
    }
}