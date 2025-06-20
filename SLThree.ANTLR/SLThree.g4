grammar SLThreeParser;

// PARSER RULES
// Начальное правило. Мы ожидаем одно выражение и конец файла (EOF).
parse: expression EOF;

// Правила для выражений с разным приоритетом.
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

NUMBER: [0-9]+;

WS: [ \t\r\n]+ -> skip; 