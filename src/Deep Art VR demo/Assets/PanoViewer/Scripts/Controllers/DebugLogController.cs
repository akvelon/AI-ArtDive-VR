using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugLogController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private ScrollRect _scrollRect;

    private readonly StringBuilder _logText = new StringBuilder();

    private void OnEnable()
    {
        ClearLog();
        Application.logMessageReceived += Application_logMessageReceived;
        Debug.Log("[DebugLogController] Log enabled");
    }

    private void OnDisable()
    {
        Debug.Log("[DebugLogController] Log disabled");
        Application.logMessageReceived -= Application_logMessageReceived;
    }

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        WriteLog(condition, stackTrace);
    }

    private void ClearLog()
    {
        _logText.Clear();
        UpdateUIText();
    }

    private void WriteLog(params string[] messages)
    {
        _logText.Insert(0, "\n");

        foreach (var message in messages)
            if (!string.IsNullOrEmpty(message))
            {
                _logText.Insert(0, "\n");
                _logText.Insert(0, message);            
            }

        UpdateUIText();
    }

    private void UpdateUIText()
    {
        try
        {
            _text.text = _logText.ToString();
            _scrollRect.verticalNormalizedPosition = 1;
        }
        catch
        {
            // may fail if text or scroll is not yet initialized, ignore
        }
    }
}
