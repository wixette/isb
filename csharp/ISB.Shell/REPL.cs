using System;

namespace ISB.Shell
{
    // A general and simple REPL (Read-Eval-Print-Loop) framework to support shell environments.
    // The framework provides the following features:
    //
    // * Simple inline editing.
    // * Histroy log (Up key to show the last input)
    // * A console spinner for time-consuming operations.
    // * A console progress bar.
    public class REPL
    {
        string prompt;

        Func<string, bool> evaluate;

        public REPL(string prompt,
            Func<string, bool> evaluate)
        {
            this.prompt = prompt;
            this.evaluate = evaluate;
        }

        public void Loop()
        {
            while (true)
            {
                Console.Write(prompt);
                string line = Console.ReadLine();
                if (!this.evaluate.Invoke(line))
                    break;
            }
        }
    }
}