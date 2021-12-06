using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ISB.Runtime;
using ISB.Utilities;
using UnityEngine.Scripting;

public class GameManager : MonoBehaviour
{
    private const int animSteps = 15;
    private const float rotateDegrees = 90;
    private const float moveDistance = 10;
    private const float interval = .05f;

    private static GameObject left;
    private static GameObject leftBall;
    private static GameObject right;
    private static GameObject rightBall;
    private static Engine ISBEngine;

    public InputField Code;
    public Text Message;

    void Start()
    {
        left = transform.Find("Left")?.gameObject;
        leftBall = left.transform.Find("Cannon")?.Find("Ball")?.gameObject;
        right = transform.Find("Right")?.gameObject;
        rightBall = right.transform.Find("Cannon")?.Find("Ball")?.gameObject;

        ISBEngine = new Engine("UnityIntegration", new Type[] { typeof(Game) });
        Game.Manager = this;
    }

    public void OnExample()
    {
        Code.text = @"For i = 1 To 3
  Game.Rotate(0)
  Game.Fire(0)
  Game.Rotate(1)
  Game.Fire(1)
EndFor";
    }

    public void OnRun()
    {
        Message.text = "";
        string code = Code.text;
        if (!ISBEngine.Compile(code, true))
        {
            ReportErrors(ISBEngine);
            return;
        }
        Action<bool> doneCallback = (isSuccess) =>
        {
            if (!isSuccess)
            {
                ReportErrors(ISBEngine);
            }
            else if (ISBEngine.StackCount > 0)
            {
                string ret = ISBEngine.StackTop.ToDisplayString();
                PrintDebugInfo($"Done: ${ret}");
            }
            else
            {
                PrintDebugInfo($"Done.");
            }
        };
        // Prevents the scripting engine from being stuck in an infinite loop.
        int maxInstructionsToExecute = 1000000;
        Func<int, bool> stepCallback = (counter) =>
        {
            PrintDebugInfo($"Current code pos: {ISBEngine.CurrentSourceTextRange}");
            return counter < maxInstructionsToExecute;
        };
        StartCoroutine(ISBEngine.RunAsCoroutine(doneCallback, stepCallback));
    }

    [Preserve]
    public void Rotate(int stationId)
    {
        StartCoroutine(RotateCoroutine(stationId));
    }

    [Preserve]
    public void Fire(int stationId)
    {
        StartCoroutine(FireCoroutine(stationId));
    }

    [Preserve]
    private IEnumerator RotateCoroutine(int stationId)
    {
        ISBEngine.PauseCoroutine();
        GetGameObjects(stationId, out GameObject station, out GameObject ball);
        if (!(station is null) && !(ball is null))
        {
            float deltaZ = rotateDegrees / animSteps;
            for (int i = 0; i < animSteps; i++)
            {
                var eulerAngles = station.transform.eulerAngles;
                station.transform.localRotation =
                    Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z + deltaZ);
                yield return new WaitForSeconds(interval);
            }
        }
        ISBEngine.ResumeCoroutine();
    }

    [Preserve]
    private IEnumerator FireCoroutine(int stationId)
    {
        ISBEngine.PauseCoroutine();
        GetGameObjects(stationId, out GameObject station, out GameObject ball);
        if (!(station is null) && !(ball is null))
        {
            ball.transform.localPosition = new Vector3(1, 0, 0);
            float deltaX = moveDistance / animSteps;
            for (int i = 0; i < animSteps; i++)
            {
                ball.transform.localPosition =
                    new Vector3(ball.transform.localPosition.x + deltaX,
                                ball.transform.localPosition.y,
                                ball.transform.localPosition.z);
                yield return new WaitForSeconds(interval);
            }
            ball.transform.localPosition = new Vector3(1, 0, 0);
        }
        ISBEngine.ResumeCoroutine();
    }

    [Preserve]
    private void GetGameObjects(int stationId, out GameObject station, out GameObject ball)
    {
        switch (stationId)
        {
            case 0:
                station = left;
                ball = leftBall;
                break;
            case 1:
                station = right;
                ball = rightBall;
                break;
            default:
                station = null;
                ball = null;
                break;
        }
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
            Message.text = "";
        }
        else
        {
            Debug.Log(message);
            Message.text = $"Debug: \n{message}";
        }
    }
}
