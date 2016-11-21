Jaro Distance Calculation
=========================
All is simple: if texts are same, distance is 1, if they are completely different, distance is 0.

May be helps, if needed.

Example of run:

```
'Quick brown fox jumps over a lazy dog' <-> 'Quick brown fox jumps over a lazy
og' = 1.00000
'Quick brown fox jumps over a lazy dog' <-> 'Quick brown fox jumps over a lazy
og' = 0.98198
'Quick brown fox jumps over a lazy dog' <-> 'Quick brown fox jumps over a quick
dog' = 0.93298
'Quick brown fox jumps over a lazy dog' <-> 'Mammy, where you go, it's just a d
eam' = 0.64390
'Abcdefgi' <-> 'Rstuwxyz' = 0.00000
>>> PRESS ENTER
```
