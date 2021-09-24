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
    nop
    load n
    push 1
    gt
    br_if __isprime.bas_4__ __isprime.bas_5__
__isprime.bas_4__:
    nop
    push 1
    store IsPrime
    br TheEnd
    br __isprime.bas_3__
__isprime.bas_5__:
    nop
    push 0
    store IsPrime
    br TheEnd
__isprime.bas_3__:
    nop
    br __isprime.bas_0__
__isprime.bas_2__:
    nop
    load n
    push 2
    mod
    push 0
    eq
    br_if __isprime.bas_9__ __isprime.bas_8__
__isprime.bas_8__:
    nop
    load n
    push 3
    mod
    push 0
    eq
    br_if __isprime.bas_9__ __isprime.bas_10__
__isprime.bas_9__:
    nop
    push 1
    br __isprime.bas_11__
__isprime.bas_10__:
    nop
    push 0
__isprime.bas_11__:
    nop
    br_if __isprime.bas_6__ __isprime.bas_7__
__isprime.bas_6__:
    nop
    push 0
    store IsPrime
    br TheEnd
    br __isprime.bas_0__
__isprime.bas_7__:
    nop
    push 5
    store i
__isprime.bas_12__:
    nop
    load i
    load i
    mul
    load n
    le
    br_if __isprime.bas_13__ __isprime.bas_14__
__isprime.bas_13__:
    nop
    load n
    load i
    mod
    push 0
    eq
    br_if __isprime.bas_19__ __isprime.bas_18__
__isprime.bas_18__:
    nop
    load n
    load i
    push 2
    add
    mod
    push 0
    eq
    br_if __isprime.bas_19__ __isprime.bas_20__
__isprime.bas_19__:
    nop
    push 1
    br __isprime.bas_21__
__isprime.bas_20__:
    nop
    push 0
__isprime.bas_21__:
    nop
    br_if __isprime.bas_16__ __isprime.bas_17__
__isprime.bas_16__:
    nop
    push 0
    store IsPrime
    br TheEnd
    br __isprime.bas_15__
__isprime.bas_17__:
    nop
__isprime.bas_15__:
    nop
    load i
    push 6
    add
    store i
    br __isprime.bas_12__
__isprime.bas_14__:
    nop
    push 1
    store IsPrime
__isprime.bas_0__:
    nop
TheEnd:
    nop
    load IsPrime
    push 1
    eq
    br_if __isprime.bas_23__ __isprime.bas_24__
__isprime.bas_23__:
    nop
    load n
    pushs " is a prime number"
    add
    call_lib BuiltIn print
    br __isprime.bas_22__
__isprime.bas_24__:
    nop
    load n
    pushs " is not a prime number"
    add
    call_lib BuiltIn print
__isprime.bas_22__:
    nop

