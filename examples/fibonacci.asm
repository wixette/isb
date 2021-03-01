;---------------------------------------------------------------------------------------------------
; The ISB Assembly code generated from fibonacci.bas
; The code can be parsed and run by the shell tool of ISB (Interactive Small Basic).
; See https://github.com/wixette/isb for more details.
;---------------------------------------------------------------------------------------------------
    push 100
    store NUM
    push 0
    push 0
    store_arr Fib 1
    push 1
    push 1
    store_arr Fib 1
    push 2
    store i
__Program_0__:
    nop
    load i
    push 1
    sub
    load_arr Fib 1
    load i
    push 2
    sub
    load_arr Fib 1
    add
    load i
    store_arr Fib 1
    push 1
    set 0
    load i
    get 0
    add
    store i
    get 0
    push 0
    ge
    br_if __Program_2__ __Program_3__
__Program_2__:
    nop
    load i
    load NUM
    le
    br_if __Program_0__ __Program_1__
__Program_3__:
    nop
    load i
    load NUM
    ge
    br_if __Program_0__ __Program_1__
__Program_1__:
    nop

