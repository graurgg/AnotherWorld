using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class APIManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public Button sendButton;
    public Transform chatHistoryContent;
    public GameObject chatMessagePrefab;

    [Header("Scrolling")]
    public ScrollRect scrollRect;

    [Header("NPC")]
    public string npcId;

    [Header("API Settings")]
    private string apiKey;
    private string apiUrl = "https://api.deepseek.com/chat/completions";

    private List<Message> conversationHistory = new List<Message>();

    [System.Serializable]
    public class Message { public string role; public string content; }
    [System.Serializable]
    public class ChatRequest { public string model; public List<Message> messages; }
    [System.Serializable]
    public class ChatResponse { public List<Choice> choices; }
    [System.Serializable]
    public class Choice { public Message message; }

    void Awake()
    {
        var secrets = Resources.Load<TextAsset>("ApiSecrets");
        if (secrets != null)
            apiKey = secrets.text.Trim();
        else
            Debug.LogError("[APIManager] ApiSecrets.txt not found in Resources. Create Assets/Resources/ApiSecrets.txt containing only your API key.");
    }

    void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);
        inputField.onSubmit.AddListener(delegate { OnSendButtonClicked(); });

        string systemPrompt = BuildSystemPrompt();
        conversationHistory.Add(new Message { role = "system", content = systemPrompt });

        inputField.ActivateInputField();
    }

    void OnSendButtonClicked()
    {
        string userText = inputField.text;
        if (string.IsNullOrWhiteSpace(userText)) return;

        AddMessageToUI("Player: " + userText);

        conversationHistory.Add(new Message { role = "user", content = userText });

        inputField.text = "";

        inputField.ActivateInputField();

        // Disable the send button to prevent multiple clicks while waiting for the response
        sendButton.interactable = false;

        StartCoroutine(SendDeepSeekRequest());
    }

    IEnumerator SendDeepSeekRequest()
    {
        ChatRequest requestData = new ChatRequest
        {
            model = "deepseek-chat",
            messages = conversationHistory
        };

        string jsonData = JsonUtility.ToJson(requestData);

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Error: " + request.error);
                AddMessageToUI("System: The connection was lost.");
            }
            else
            {
                ChatResponse responseData = JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);
                if (responseData != null && responseData.choices != null && responseData.choices.Count > 0)
                {
                    string rawText = responseData.choices[0].message.content;
                    Debug.Log($"[APIManager] Raw response from '{npcId}':\n{rawText}");
                    var parsed = LLMResponseParser.Parse(rawText);

                    if (parsed.hasWarning)
                        Debug.LogWarning("[APIManager] LLM issued a WARNING tag for NPC: " + npcId);

                    foreach (int keyId in parsed.detectedKeys)
                        GameStateManager.Instance?.CollectKey(keyId);

                    conversationHistory.Add(new Message { role = "assistant", content = parsed.cleanedText });
                    AddMessageToUI("NPC: " + parsed.cleanedText);
                }
            }

            // Re-enable the send button
            sendButton.interactable = true;
        }
    }

    public void BeginConversation(string newNpcId)
    {
        npcId = newNpcId;
        conversationHistory.Clear();
        conversationHistory.Add(new Message { role = "system", content = BuildSystemPrompt() });

        foreach (Transform child in chatHistoryContent)
            if (!ReferenceEquals(child.gameObject, chatMessagePrefab))
                Destroy(child.gameObject);

        sendButton.interactable = true;
        inputField.text = "";
        inputField.ActivateInputField();
    }

    string BuildSystemPrompt()
    {
        if (!string.IsNullOrWhiteSpace(npcId))
        {
            NPCData npc = NPCDataLoader.Load(npcId);
            if (npc != null)
            {
                string prompt = SystemPromptBuilder.Build(npc);
                Debug.Log($"[APIManager] System prompt for '{npcId}':\n{prompt}");
                return prompt;
            }
            Debug.LogWarning("[APIManager] NPC data not found for id: " + npcId + ". Falling back to generic prompt.");
        }
        return "You are a villager in the medieval town of Saltmere. Keep your answers brief and in character.";
    }

    void AddMessageToUI(string text)
    {
        GameObject newMsg = Instantiate(chatMessagePrefab, chatHistoryContent);
        newMsg.GetComponent<TextMeshProUGUI>().text = text;

        StartCoroutine(ScrollToBottom());
    }

    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();

        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }

        // second forced update for the ScrollRect specifically
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        scrollRect.verticalNormalizedPosition = 0f;
    }
}