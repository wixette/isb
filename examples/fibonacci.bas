' Small Basic Code to generate Fibonacci sequence.
NUM = 100
Fib[0] = 0
Fib[1] = 1
For i = 2 to NUM
    Fib[i] = Fib[i - 1] + Fib[i - 2]
EndFor
