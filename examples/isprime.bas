n = 1000117 ' number to be test.
IsPrime = 0
if n <= 3 then
  if n > 1 then
    IsPrime = 1
    goto TheEnd
  else
    IsPrime = 0
    goto TheEnd
  endif
elseif n mod 2 = 0 or n mod 3 = 0 then
  IsPrime = 0
  goto TheEnd
else
  i = 5
  while i * i <= n
    if n mod i = 0 or n mod (i + 2) = 0 then
      IsPrime = 0
      goto TheEnd
    endif
    i = i + 6
  endwhile
  IsPrime = 1
endif
TheEnd:
