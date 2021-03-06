using System;
using System.Threading.Tasks;

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

        public interface IEvaluator
        {
            public EvalResult Eval(string line, out Task incompleteTask);
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
        }

        public void Loop()
        {
            while (true)
            {
                Console.Write(secondLevel ? secondLevelPrompt : prompt);
                string line = Console.ReadLine();
                if (line == null)
                {
                    Console.WriteLine();
                    break;
                }

                EvalResult result = this.evaluator.Eval(line, out Task incompleteTask);
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
                else if (result == EvalResult.Wait && incompleteTask != null)
                {
                    // TODO
                    secondLevel = false;
                    continue;
                }
            }
        }
    }
}