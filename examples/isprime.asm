;---------------------------------------------------------------------------------------------------
; The ISB Assembly code generated from isprime.bas
; The code can be parsed and run by the shell tool of ISB (Interactive Small Basic).
; See https://github.com/wixette/isb for more details.
;---------------------------------------------------------------------------------------------------
    push 1000117
    store n
    push 0
    store IsPrime
    load n
    push 3
    le
    br_if __Program_1__ __Program_2__
__Program_1__:
    nop
    load n
    push 1
    gt
    br_if __Program_4__ __Program_5__
__Program_4__:
    nop
    push 1
    store IsPrime
    br TheEnd
    br __Program_3__
__Program_5__:
    nop
    push 0
    store IsPrime
    br TheEnd
__Program_3__:
    nop
    br __Program_0__
__Program_2__:
    nop
    load n
    push 2
    mod
    push 0
    eq
    br_if __Program_9__ __Program_8__
__Program_8__:
    nop
    load n
    push 3
    mod
    push 0
    eq
    br_if __Program_9__ __Program_10__
__Program_9__:
    nop
    push 1
    br __Program_11__
__Program_10__:
    nop
    push 0
__Program_11__:
    nop
    br_if __Program_6__ __Program_7__
__Program_6__:
    nop
    push 0
    store IsPrime
    br TheEnd
    br __Program_0__
__Program_7__:
    nop
    push 5
    store i
__Program_12__:
    nop
    load i
    load i
    mul
    load n
    le
    br_if __Program_13__ __Program_14__
__Program_13__:
    nop
    load n
    load i
    mod
    push 0
    eq
    br_if __Program_19__ __Program_18__
__Program_18__:
    nop
    load n
    load i
    push 2
    add
    mod
    push 0
    eq
    br_if __Program_19__ __Program_20__
__Program_19__:
    nop
    push 1
    br __Program_21__
__Program_20__:
    nop
    push 0
__Program_21__:
    nop
    br_if __Program_16__ __Program_17__
__Program_16__:
    nop
    push 0
    store IsPrime
    br TheEnd
    br __Program_15__
__Program_17__:
    nop
__Program_15__:
    nop
    load i
    push 6
    add
    store i
    br __Program_12__
__Program_14__:
    nop
    push 1
    store IsPrime
__Program_0__:
    nop
TheEnd:
    nop

