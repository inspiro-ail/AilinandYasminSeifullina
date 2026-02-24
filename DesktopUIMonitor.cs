using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DesktopUIMonitor : MonoBehaviour
{
    private Canvas desktopCanvas;
    private TextMeshProUGUI transcriptionText;
    private TextMeshProUGUI responseText;
    private TextMeshProUGUI statusText;
    private TextMeshProUGUI microphoneText;
    private TMP_Dropdown microphoneDropdown;
    
    private VRAssistantManager manager;
    
    void Start()
    {
        CreateDesktopUI();
    }
    
    public void SetManagerReference(VRAssistantManager managerRef)
    {
        manager = managerRef;
        UpdateMicrophoneDisplay();
    }
    
    void CreateDesktopUI()
    {
        GameObject canvasGO = new GameObject("Desktop_Monitor_Canvas");
        desktopCanvas = canvasGO.AddComponent<Canvas>();
        desktopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        desktopCanvas.sortingOrder = 999;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        GameObject panelGO = new GameObject("Background");
        panelGO.transform.SetParent(canvasGO.transform, false);
        
        Image bgImage = panelGO.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.9f);
        
        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(0.4f, 1);
        panelRect.sizeDelta = Vector2.zero;
        
        VerticalLayoutGroup layout = panelGO.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(30, 30, 30, 30);
        layout.spacing = 15;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = false;
        
        // Title
        CreateText(panelGO.transform, "🎤 VR ASSISTANT DEBUG MONITOR", 28, Color.yellow, TextAlignmentOptions.Center);
        
        CreateSeparator(panelGO.transform);
        
        // Microphone section
        CreateText(panelGO.transform, "🎙️ MICROPHONE:", 18, new Color(1f, 0.5f, 0f));
        microphoneText = CreateText(panelGO.transform, "Detecting...", 16, Color.white);
        microphoneText.GetComponent<LayoutElement>().preferredHeight = 60;
        
        // Microphone dropdown
        CreateMicrophoneDropdown(panelGO.transform);
        
        CreateSeparator(panelGO.transform);
        
        // Status
        statusText = CreateText(panelGO.transform, "Status: Initializing...", 20, Color.white);
        statusText.GetComponent<LayoutElement>().preferredHeight = 40;
        
        CreateSeparator(panelGO.transform);
        
        // Transcription
        CreateText(panelGO.transform, "📝 YOU SAID:", 18, new Color(0.5f, 1f, 1f));
        transcriptionText = CreateText(panelGO.transform, "Waiting for input...", 22, Color.white);
        transcriptionText.GetComponent<LayoutElement>().preferredHeight = 150;
        
        CreateSeparator(panelGO.transform);
        
        // Response
        CreateText(panelGO.transform, "🤖 AI RESPONDED:", 18, new Color(0.5f, 1f, 0.5f));
        responseText = CreateText(panelGO.transform, "Waiting for response...", 22, Color.white);
        responseText.GetComponent<LayoutElement>().preferredHeight = 220;
        
        CreateSeparator(panelGO.transform);
        
        // Instructions
        CreateText(panelGO.transform, "💡 HOLD TRIGGER TO TALK", 16, Color.gray, TextAlignmentOptions.Center);
        CreateText(panelGO.transform, "Response will be GREEN (Russian) or RED (English)", 12, Color.gray, TextAlignmentOptions.Center);
        
        Debug.Log("✅ Desktop monitor UI created!");
    }
    
    void CreateMicrophoneDropdown(Transform parent)
    {
        GameObject dropdownGO = new GameObject("MicrophoneDropdown");
        dropdownGO.transform.SetParent(parent, false);
        
        // Create dropdown background
        Image dropdownBg = dropdownGO.AddComponent<Image>();
        dropdownBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        microphoneDropdown = dropdownGO.AddComponent<TMP_Dropdown>();
        
        // Setup dropdown
        RectTransform dropdownRect = dropdownGO.GetComponent<RectTransform>();
        dropdownRect.sizeDelta = new Vector2(0, 40);
        
        LayoutElement dropdownLayout = dropdownGO.AddComponent<LayoutElement>();
        dropdownLayout.preferredHeight = 40;
        
        // Populate with microphones
        microphoneDropdown.ClearOptions();
        List<string> options = new List<string>();
        
        if (Microphone.devices.Length > 0)
        {
            foreach (string device in Microphone.devices)
            {
                options.Add(device);
            }
        }
        else
        {
            options.Add("No microphones detected");
        }
        
        microphoneDropdown.AddOptions(options);
        microphoneDropdown.onValueChanged.AddListener(OnMicrophoneChanged);
        
        // Create label and arrow (dropdown template)
        CreateDropdownTemplate(dropdownGO);
    }
    
    void CreateDropdownTemplate(GameObject dropdownGO)
    {
        // Label
        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(dropdownGO.transform, false);
        
        TextMeshProUGUI labelText = labelGO.AddComponent<TextMeshProUGUI>();
        labelText.text = "Select Microphone";
        labelText.fontSize = 14;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.MidlineLeft;
        
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(10, 0);
        labelRect.offsetMax = new Vector2(-30, 0);
        
        microphoneDropdown.captionText = labelText;
    }
    
    void OnMicrophoneChanged(int index)
    {
        if (index >= 0 && index < Microphone.devices.Length)
        {
            string selectedMic = Microphone.devices[index];
            Debug.Log($"📱 User selected microphone: {selectedMic}");
            
            if (manager != null)
            {
                manager.SetMicrophone(selectedMic);
                UpdateMicrophoneDisplay();
            }
        }
    }
    
    void UpdateMicrophoneDisplay()
    {
        if (manager != null && microphoneText != null)
        {
            string currentMic = manager.GetCurrentMicrophone();
            
            if (!string.IsNullOrEmpty(currentMic))
            {
                bool isVRMic = currentMic.ToLower().Contains("oculus") || 
                               currentMic.ToLower().Contains("quest") || 
                               currentMic.ToLower().Contains("virtual");
                
                microphoneText.text = $"Active: {currentMic}\n{(isVRMic ? "✅ VR Headset Mic" : "⚠️ PC Microphone")}";
                microphoneText.color = isVRMic ? Color.green : Color.yellow;
            }
            else
            {
                microphoneText.text = "No microphone selected";
                microphoneText.color = Color.red;
            }
        }
    }
    
    TextMeshProUGUI CreateText(Transform parent, string text, int fontSize, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft)
    {
        GameObject textGO = new GameObject("Text_" + text.Substring(0, Mathf.Min(10, text.Length)));
        textGO.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.textWrappingMode = TMPro.TextWrappingModes.Normal;
        tmp.alignment = alignment;
        
        LayoutElement layoutElement = textGO.AddComponent<LayoutElement>();
        layoutElement.flexibleHeight = 1;
        
        return tmp;
    }
    
    void CreateSeparator(Transform parent)
    {
        GameObject sepGO = new GameObject("Separator");
        sepGO.transform.SetParent(parent, false);
        
        Image img = sepGO.AddComponent<Image>();
        img.color = new Color(1, 1, 1, 0.3f);
        
        LayoutElement layout = sepGO.AddComponent<LayoutElement>();
        layout.preferredHeight = 2;
    }
    
    public void UpdateTranscription(string text)
    {
        if (transcriptionText != null)
        {
            transcriptionText.text = text;
            transcriptionText.color = Color.white;
        }
    }
    
    public void UpdateResponse(string text)
    {
        if (responseText != null)
        {
            responseText.text = text;
            
            bool isRussian = ContainsRussian(text);
            responseText.color = isRussian ? Color.green : Color.red;
        }
    }
    
    public void UpdateStatus(string text)
    {
        if (statusText != null)
        {
            statusText.text = $"Status: {text}";
        }
        
        UpdateMicrophoneDisplay();
    }
    
    bool ContainsRussian(string text)
    {
        foreach (char c in text)
        {
            if (c >= 0x0400 && c <= 0x04FF)
                return true;
        }
        return false;
    }
}