# MoreStaticAnalisis
This package is meant to provide static analisys for typical and easily diagnosible errors in C# code. Currently suported diagnostics:
* Enum with ``Flags`` attribute has values that are not powers of two. (If you want to have enum values that are a combinations of other values like
```
enum Subjects
{
  Math = 1,
  Physics = 2,
  History = 4,
  All = Math | Physics | History
}
```
mark ``All`` with the ``CombinedFlags`` attribute from ``MoreStaticAnalisis.Attributes`` namespace)
* Enum with ``Flags`` attribute has a value equal zero. For example:
```
enum Subjects
{
  Math,
  None
  Physics = 2,
  History = 4,
}
```
``Math`` has value 0 and ``None`` value 1. To fix the warning, assign the desired label values and mark ``None`` with ``None`` attribute from ``MoreStaticAnalisis.Attributes`` namespace:
```
enum Subjects
{
  Math = 1,
  [None]
  None = 0
  Physics = 2,
  History = 4,
}
```

Feedback via github issues is always welcome.
