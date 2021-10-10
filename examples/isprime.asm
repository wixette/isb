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
    br_if __isprime.bas_1__ __isprime.bas_2__
__isprime.bas_1__:
    load n
    push 1
    gt
    br_if __isprime.bas_4__ __isprime.bas_5__
__isprime.bas_4__:
    push 1
    store IsPrime
    br TheEnd
    br __isprime.bas_3__
__isprime.bas_5__:
    push 0
    store IsPrime
    br TheEnd
__isprime.bas_3__:
    br __isprime.bas_0__
__isprime.bas_2__:
    load n
    push 2
    mod
    push 0
    eq
    br_if __isprime.bas_9__ __isprime.bas_8__
__isprime.bas_8__:
    load n
    push 3
    mod
    push 0
    eq
    br_if __isprime.bas_9__ __isprime.bas_10__
__isprime.bas_9__:
    push 1
    br __isprime.bas_11__
__isprime.bas_10__:
    push 0
__isprime.bas_11__:
    br_if __isprime.bas_6__ __isprime.bas_7__
__isprime.bas_6__:
    push 0
    store IsPrime
    br TheEnd
    br __isprime.bas_0__
__isprime.bas_7__:
    push 5
    store i
__isprime.bas_12__:
    load i
    load i
    mul
    load n
    le
    br_if __isprime.bas_13__ __isprime.bas_14__
__isprime.bas_13__:
    load n
    load i
    mod
    push 0
    eq
    br_if __isprime.bas_19__ __isprime.bas_18__
__isprime.bas_18__:
    load n
    load i
    push 2
    add
    mod
    push 0
    eq
    br_if __isprime.bas_19__ __isprime.bas_20__
__isprime.bas_19__:
    push 1
    br __isprime.bas_21__
__isprime.bas_20__:
    push 0
__isprime.bas_21__:
    br_if __isprime.bas_16__ __isprime.bas_17__
__isprime.bas_16__:
    push 0
    store IsPrime
    br TheEnd
    br __isprime.bas_15__
__isprime.bas_17__:
    nop
__isprime.bas_15__:
    load i
    push 6
    add
    store i
    br __isprime.bas_12__
__isprime.bas_14__:
    push 1
    store IsPrime
__isprime.bas_0__:
    nop
TheEnd:
    load IsPrime
    push 1
    eq
    br_if __isprime.bas_23__ __isprime.bas_24__
__isprime.bas_23__:
    load n
    pushs " is a prime number"
    add
    call_lib BuiltIn print
    br __isprime.bas_22__
__isprime.bas_24__:
    load n
    pushs " is not a prime number"
    add
    call_lib BuiltIn print
__isprime.bas_22__:
    nop

