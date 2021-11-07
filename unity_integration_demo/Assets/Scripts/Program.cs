using System;
using System.Collections.Generic;
using ISB.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class Foo
{
    // A library function that starts an inner ISB Engine.
    public StringValue Bar(StringValue code)
    {
        if (code.ToString().Length <= 0)
        {
            // If no code is passed in, a prime check example will be executed.
            code = new StringValue(@"n = 1000117 ' number to be test.
                  IsPrime = False
                  if n <= 3 then
                    if n > 1 then
                      IsPrime = True
                      goto TheEnd
                    else
                      IsPrime = False
                      goto TheEnd
                    endif
                  elseif n mod 2 = 0 or n mod 3 = 0 then
                    IsPrime = False
                    goto TheEnd
                  else
                    i = 5
                    while i * i <= n
                      if n mod i = 0 or n mod (i + 2) = 0 then
                        IsPrime = False
                        goto TheEnd
                      endif
                      i = i + 6
                    endwhile
                    IsPrime = True
                  endif
                  TheEnd:
                  IsPrime");
        }
        Debug.Log($"Foo.Bar, running:\n{code}");
        Engine engine = new Engine("Foo.Bar", new Type[] { typeof(Game) });
        if (engine.Compile(code.ToString(), true) &&
            engine.Run(true) &&
            engine.StackCount > 0)
        {
                return new StringValue(engine.StackTop.ToDisplayString());
        }
        return StringValue.Empty;
    }
}

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

        Engine engine = new Engine("Unity", new Type[] { typeof(Game), typeof(Foo) });
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
