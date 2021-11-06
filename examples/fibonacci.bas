' ISB sample code to generate Fibonacci sequence.
NUM = 30
Fib[0] = 0
Fib[1] = 1
for i = 2 to NUM
  Fib[i] = Fib[i - 1] + Fib[i - 2]
endfor
for i = 0 to NUM
  print(Fib[i])
endfor
