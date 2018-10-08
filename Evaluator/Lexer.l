%namespace Parser
%option out:Generated/Lexer.cs
%using QUT.Gppg;

alpha [a-zA-Z]
digit [0-9]
alphanum {alpha}|{digit}

%%

" "|\r|\n|\r\n   { }
\/\/.*$       { }
\t			{ }


"if"     { return (int) Tokens.IF; }
"while"         { return (int) Tokens.WHILE; }
"return"        { return (int) Tokens.RETURN; }
"int"         { return (int) Tokens.INT; }
"bool"         { return (int) Tokens.BOOL; }
"void"         { return (int) Tokens.VOID; }
"else"			{ return (int) Tokens.ELSE; }

";"         { return (int) Tokens.SEMI; }
"("         { return (int) Tokens.LPAR; }
")"         { return (int) Tokens.RPAR; }
","         { return (int) Tokens.COMMA; }
"{"         { return (int) Tokens.LCURLY; }
"}"         { return (int) Tokens.RCURLY; }

"="         { return (int) Tokens.ASN; }
"||"         { return (int) Tokens.OR; }
"&&"         { return (int) Tokens.AND; }
"=="         { return (int) Tokens.EQ; }
"!="         { return (int) Tokens.NOTEQ; }
"<"         { return (int) Tokens.LESS; }
">"         { return (int) Tokens.MORE; }
"<="         { return (int) Tokens.LESSEQ; }
">="         { return (int) Tokens.MOREEQ; }
"+"         { return (int) Tokens.PLUS; }
"-"         { return (int) Tokens.MINUS; }
"*"         { return (int) Tokens.MULTI; }
"/"         { return (int) Tokens.SLASH; }
"!"         { return (int) Tokens.EXCLA; }

"true"         { return (int) Tokens.TRUE; }
"false"         { return (int) Tokens.FALSE; }


{alpha}{alphanum}* { yylval.value = yytext;
                     return (int) Tokens.ID;
                   }

{digit}+    { yylval.value = yytext;
              return (int) Tokens.NUM; 
            }

.           { yylval.value = yytext;
              return (int) Tokens.LEXERR;
            }

%{
   yylloc = new LexLocation(tokLin, tokCol, tokELin, tokECol); 
%}

%%

override public void yyerror(string msg, object[] args) {
  Console.WriteLine("{0} at line {1} column {2}",
    msg, yylloc.StartLine, yylloc.StartColumn
  );
}