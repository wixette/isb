using System;
using System.Collections.Generic;
using ISB.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public InputField Code;
    public Text DebugInfo;
    public GameObject objBall;

    public void OnRun()
    {
        string code = Code.text;
        DebugInfo.text = "";
        Engine engine = new Engine("UnityDemo", new Type[] { typeof(Game) });
        if (!engine.Compile(code, true))
        {
            ReportErrors(engine);
            return;
        }
        // Runs the program in a Unity coroutine.
        Action<bool> doneCallback = (isSuccess) =>
        {
            if (!isSuccess)
            {
                ReportErrors(engine);
            }
            else if (engine.StackCount > 0)
            {
                string ret = engine.StackTop.ToDisplayString();
                PrintDebugInfo(ret);
            }
        };
        // Prevents the scripting engine from being stuck in an infinite loop.
        int maxInstructionsToExecute = 1000000;
        Func<int, bool> stepCallback =
            (counter) => counter >= maxInstructionsToExecute ? false : true;
        StartCoroutine(engine.RunAsCoroutine(doneCallback, stepCallback));
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
