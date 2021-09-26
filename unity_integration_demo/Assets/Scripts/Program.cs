using System;
using System.Collections.Generic;
using ISB.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class Program : MonoBehaviour
{
    public Button uiButton;
    public InputField uiInput;
    public Text DebugInfo;
    public GameObject objBall;

    void Start()
    {
        uiButton.onClick.AddListener(onButtonClick);
    }

    void Update()
    {
    }

    void onButtonClick()
    {
        string code = uiInput.text;
        Debug.Log(code);

        Engine engine = new Engine("Unity", new Type[] { typeof(Game) });
        if (engine.Compile(code, true) && engine.Run(true))
        {
            if (engine.StackCount > 0)
            {
                string ret = engine.StackTop.ToDisplayString();
                PrintDebugInfo(ret);
            }
            else
            {
                PrintDebugInfo(null);
            }
        }
        else
        {
            var buffer = new List<string>();
            foreach (var content in engine.ErrorInfo.Contents)
            {
                buffer.Add(content.ToDisplayString());
            }
            PrintDebugInfo(string.Join("\n", buffer));
        }
    }

    private void PrintDebugInfo(string message) {
        if (string.IsNullOrEmpty(message))
        {
            DebugInfo.text = "";
        }
        else
        {
            Debug.Log(message);
            DebugInfo.text = $"Debug: \n{message}";
        }
    }
}
