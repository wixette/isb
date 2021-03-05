using System;

namespace ISB.Shell
{
    // A general and simple REPL (Read-Eval-Print-Loop) framework to support shell environments.
    // The framework provides the following features:
    //
    // * Simple inline editing.
    // * History log. Press Up key to show the last input.
    // * A console spinner for time-consuming operations.
    public class REPL
    {
        public enum EvalResult
        {
            OK,
            Exit,
            NeedMoreLines,
            Wait
        }

        string prompt;
        string secondLevelPrompt;

        Func<string, EvalResult> evaluate;

        public REPL(string prompt,
            string secondLevelPrompt,
            Func<string, EvalResult> evaluate)
        {
            this.prompt = prompt;
            this.secondLevelPrompt = secondLevelPrompt;
            this.evaluate = evaluate;
        }

        public void Loop()
        {
            while (true)
            {
                Console.Write(prompt);
                string line = Console.ReadLine();
                EvalResult result = this.evaluate.Invoke(line);
                if (result == EvalResult.OK)
                {
                    continue;
                }
                else if (result == EvalResult.Exit)
                {
                    break;
                }
                else if (result == EvalResult.NeedMoreLines)
                {
                    // TODO
                }
                else if (result == EvalResult.Wait)
                {
                    // TODO
                }
            }
        }
    }
}