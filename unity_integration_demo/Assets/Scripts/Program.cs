using System;
using ISB.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class Program : MonoBehaviour
{
    public Button uiButton;
    public InputField uiInput;
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
                Debug.Log(ret);
            }
        }
        else
        {
            foreach (var content in engine.ErrorInfo.Contents)
            {
                Debug.Log(content.ToDisplayString());
            }
        }
    }
}
