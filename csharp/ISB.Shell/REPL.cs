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
            NeedMoreLines
        }

        public interface IEvaluator
        {
            public EvalResult Eval(string line);
        }

        string prompt;
        string secondLevelPrompt;
        IEvaluator evaluator;
        bool secondLevel = false;

        public REPL(string prompt,
            string secondLevelPrompt,
            IEvaluator evaluator)
        {
            this.prompt = prompt;
            this.secondLevelPrompt = secondLevelPrompt;
            this.evaluator = evaluator;
            ReadLine.HistoryEnabled = true;
        }

        public void Loop()
        {
            while (true)
            {
                string line = ReadLine.Read(secondLevel ? secondLevelPrompt : prompt);
                if (line == null)
                {
                    Console.WriteLine();
                    break;
                }

                EvalResult result = this.evaluator.Eval(line);
                if (result == EvalResult.OK)
                {
                    secondLevel = false;
                    continue;
                }
                else if (result == EvalResult.Exit)
                {
                    break;
                }
                else if (result == EvalResult.NeedMoreLines)
                {
                    secondLevel = true;
                    continue;
                }
            }
        }
    }
}