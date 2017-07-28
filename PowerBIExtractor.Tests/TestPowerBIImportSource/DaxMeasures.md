MeasuresTest
=============================

```DAX
Number Of People Named John =var calculatedValue = 
CALCULATE(
COUNT(Person[Name]), FILTER(Person, Person[Name] = "John" )
)
return IF(ISBLANK(calculatedValue), 0, calculatedValue)
```

```DAX
Number Of People Named Bob =
var calculatedValue = 
CALCULATE(
COUNT(Person[Name]), FILTER(Person, Person[Name] = "Bob" )
)
return IF(ISBLANK(calculatedValue), 0, calculatedValue)
```
