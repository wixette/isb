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

    void onButtonClick()
    {
        string code = uiInput.text;
        Debug.Log(code);

        Engine engine = new Engine("Unity", new Type[] { typeof(Game) });
        if (!engine.Compile(code, true))
        {
            ReportErrors(engine);
            return;
        }

        // Runs the program in a Unity coroutine.
        Action<bool> doneCallback = (value) =>
        {
            if (!value)
            {
                ReportErrors(engine);
            }
            else if (engine.StackCount > 0)
            {
                string ret = engine.StackTop.ToDisplayString();
                PrintDebugInfo(ret);
            }
        };
        // Prevents the scripting engine to be stuck in an infinite loop.
        int maxInstructionsToExecute = 1000000;
        Func<int, bool> canContinueCallback =
            (counter) => counter >= maxInstructionsToExecute ? false : true;
        StartCoroutine(engine.RunAsCoroutine(doneCallback, canContinueCallback));
    }

    private void ReportErrors(Engine engine)
    {
        var buffer = new List<string>();
        foreach (var content in engine.ErrorInfo.Contents)
        {
            buffer.Add(content.ToDisplayString());
        }
        PrintDebugInfo(string.Join("\n", buffer));
    }

    private void PrintDebugInfo(string message)
    {
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
