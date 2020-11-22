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

Feedback via github issues is always welcome.
