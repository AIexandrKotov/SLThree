# SLThree

.NET Framework script programming language.

For more information about language: [wiki/Language](https://github.com/AIexandrKotov/SLThree/wiki/Language)

### Powerful REPL

![image](https://github.com/AIexandrKotov/SLThree/assets/44296606/8f6fb9df-54c3-42aa-835a-6fbb35c93e85)

### Embedding
SLThree may be runned in any .NET Framework language. Here code example for PascalABC.NET:

```pas
{$reference 'Pegasus.Common.dll'}
{$reference 'SLThree.dll'}

uses SLThree.Embedding;

type
  Adder = auto class
    public L: integer;
    public R: integer;
    
    public constructor() := exit;
  end;

begin
  'L = 3i32; R = 5i32;'.RunScript().Unwrap&<Adder>().ToString().Println(); //(3,5)
  'L = 2 + 2 * 2 as i32; R = 78 as i32;'.RunScript().Unwrap&<Adder>().ToString().Println();; //6, 78
end.
```

### LANG and REPL compatibility

| REPL version    | LANG version    |
|-----------------|-----------------|
| 1.1.0           | 0.2.0+          |
| 1.0.0           | 0.2.0+          |

### Download
[![stable](https://img.shields.io/badge/REPL_stable-1.1.0-00cc00)](https://github.com/AIexandrKotov/SLThree/releases/tag/0.4.0) [![stable](https://img.shields.io/badge/LANG_exp-0.5.1-ccaa00)](https://github.com/AIexandrKotov/SLThree/releases/tag/0.5.1)