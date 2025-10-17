using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class ConsoleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText;
    [SerializeField] private int maxLineCount = 10;
    
    int linecount = 0;
    private string myLog;


    void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        string logColor = type switch
        {
            LogType.Warning => "yellow",
            LogType.Exception or LogType.Error => "red",
            _ => "white"
        };
        
        logString = "<color=" + logColor + ">" + logString + "</color>";
        
        myLog = myLog + "\n" + logString;
        linecount++;

        if (linecount > maxLineCount)
        {
            linecount --;
            
            myLog  = DeleteLines(myLog, 1);
        }
        
        consoleText.text = myLog;
    }


    string DeleteLines(string message, int linesToDelete)
    {
        
        return message.Split(Environment.NewLine.ToCharArray(), linesToDelete + 1).Skip(linesToDelete).FirstOrDefault();
    }
}
