;---------------------------------------------------------------------------------------------------
; The ISB Assembly code generated from fibonacci.bas
; The code can be parsed and run by the shell tool of ISB (Interactive Small Basic).
; See https://github.com/wixette/isb for more details.
;---------------------------------------------------------------------------------------------------
    push 30
    store NUM
    push 0
    push 0
    store_arr Fib 1
    push 1
    push 1
    store_arr Fib 1
    push 2
    store i
__fibonacci.bas_0__:
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
    br_if __fibonacci.bas_2__ __fibonacci.bas_3__
__fibonacci.bas_2__:
    nop
    load i
    load NUM
    le
    br_if __fibonacci.bas_0__ __fibonacci.bas_1__
__fibonacci.bas_3__:
    nop
    load i
    load NUM
    ge
    br_if __fibonacci.bas_0__ __fibonacci.bas_1__
__fibonacci.bas_1__:
    nop
    push 0
    store i
__fibonacci.bas_4__:
    nop
    load i
    load_arr Fib 1
    call_lib BuiltIn print
    push 1
    set 0
    load i
    get 0
    add
    store i
    get 0
    push 0
    ge
    br_if __fibonacci.bas_6__ __fibonacci.bas_7__
__fibonacci.bas_6__:
    nop
    load i
    load NUM
    le
    br_if __fibonacci.bas_4__ __fibonacci.bas_5__
__fibonacci.bas_7__:
    nop
    load i
    load NUM
    ge
    br_if __fibonacci.bas_4__ __fibonacci.bas_5__
__fibonacci.bas_5__:
    nop

