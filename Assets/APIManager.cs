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

    [Header("API Settings")]
    public string apiKey = "sk-eca0e7c8c873405696e68ef3d8880265"; // TODO: Move this to a secure location in production
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

    void Start()
    {
        sendButton.onClick.AddListener(OnSendButtonClicked);

        inputField.onSubmit.AddListener(delegate { OnSendButtonClicked(); });

        conversationHistory.Add(new Message
        {
            role = "system",
            content = "You are a villager in the medieval town of Saltmere. Keep your answers brief and in character."
        });
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
                    string aiText = responseData.choices[0].message.content;

                    conversationHistory.Add(new Message { role = "assistant", content = aiText });

                    AddMessageToUI("NPC: " + aiText);
                }
            }

            // Re-enable the send button
            sendButton.interactable = true;
        }
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