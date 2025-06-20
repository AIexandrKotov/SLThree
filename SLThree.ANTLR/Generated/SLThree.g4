// SLThree/ANTLR/SLThree.g4
grammar SLThree;

// PARSER RULES
// Начальное правило. Мы ожидаем одно выражение и конец файла (EOF).
parse: expression EOF;

// Правила для выражений с разным приоритетом.
// `expression` - самый низкий приоритет (сложение/вычитание).
// `term` - средний приоритет (умножение/деление).
// `factor` - самый высокий приоритет (числа, скобки).
// Такая структура гарантирует правильный порядок операций.
expression
    : term ( (PLUS | MINUS) term )*
    ;

term
    : factor ( (MUL | DIV) factor )*
    ;

factor
    : LPAREN expression RPAREN
    | NUMBER
    ;


// LEXER RULES
LPAREN: '(';
RPAREN: ')';
PLUS:   '+';
MINUS:  '-';
MUL:    '*';
DIV:    '/';

// Распознаем целые числа
NUMBER: [0-9]+;

// Пропускаем пробелы, табуляции и переводы строк
WS: [ \t\r\n]+ -> skip; 