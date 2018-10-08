%namespace Parser
%output=Generated/Parser.cs

%union {
   public String value;
   public Declaration D;
   public Statement S;
   public Expression E;
   public Declaration.Type type;
   public List<Statement> F;
   public List<Expression> L;
   public List<TypeDeclaration> R;
}

%type <D> S, T
%type <S> A
%type <type> M
%type <F> B
%type <E> E, N, P1, P2, P3, P4, P5, P6, P7, P8
%type <L> L
%type <R> R

%token <value> ID
%token <value> NUM


%token IF "if"
%token WHILE "while"
%token RETURN "return"
%token INT "int"
%token BOOL "bool"
%token VOID "void" 
%token ELSE "else"

%token SEMI ";"
%token LPAR "("
%token RPAR ")"
%token COMMA ","
%token LCURLY "{"
%token RCURLY "}"

%token ASN "="
%token OR "\|\|"
%token AND "&&"
%token EQ "=="
%token NOTEQ "!="
%token LESS "<"
%token MORE ">"
%token LESSEQ "<="
%token MOREEQ ">="
%token PLUS "\+"
%token MINUS "-"
%token MULTI "\*"
%token SLASH "/"
%token EXCLA "!"

%token TRUE "true"
%token FALSE "false"

%token <value> LEXERR


%%



P : S EOF							{ program = $1; }
  ;

S : T S								{ $$ = new SequenceDeclaration($1, $2); $$.SetLocation(@$); }
  | T								{ $$ = $1; $$.SetLocation(@$); }
  ;

T : M ID "(" R ")" "{" B "}"		{ $$ = new FunctionDeclaration($1, new IdentifierExpression($2), $4, new BlockStatement($7)); $$.SetLocation(@$); }
  ;

R : R "," M ID						{ $$ = $1; $$.Add(new TypeDeclaration($3, new IdentifierExpression($4))); }
  |	M ID							{ $$ = new List<TypeDeclaration>(); $$.Add(new TypeDeclaration($1, new IdentifierExpression($2))); }
  |									{ $$ = new List<TypeDeclaration>(); }
  ;

M : "int"							{ $$ = Declaration.Type.INT; }
  | "bool"							{ $$ = Declaration.Type.BOOL; }
  | "void"                          { $$ = Declaration.Type.VOID; }
  ;

A : "if" "(" N ")" A				{ $$ = new ifStatement($3, $5); $$.SetLocation(@$); }			
  | "if" "(" N ")" A "else" A		{ $$ = new ifElseStatement($3, $5, $7); $$.SetLocation(@$); }
  | "while" "(" N ")" A				{ $$ = new whileStatement($3, $5); $$.SetLocation(@$); }
  | "return" ";"					{ $$ = new returnStatement(); $$.SetLocation(@$); }
  | "return" N ";"					{ $$ = new returnExprStatement($2); $$.SetLocation(@$); }
  | N ";"							{ $$ = new ExprStatement($1); $$.SetLocation(@$); }
  | M ID ";"						{ $$ = new DeclStatement(new TypeDeclaration($1, new IdentifierExpression($2))); $$.SetLocation(@$); }
  | "{" B "}"						{ $$ = new BlockStatement($2); $$.SetLocation(@$); }
  ;

B : A B								{ $$ = $2; $$.Insert(0, $1); }
  |									{ $$ = new List<Statement>(); }
  ;

E : NUM								{ $$ = new NumberExpression($1); $$.SetLocation(@$); }
  | "true"							{ $$ = new BoolExpression(true); $$.SetLocation(@$); }
  | "false"							{ $$ = new BoolExpression(false); $$.SetLocation(@$); }
  | ID								{ $$ = new IdentifierExpression($1); $$.SetLocation(@$); }
  | ID "(" L ")"					{ $$ = new FunctionCallExpression(new IdentifierExpression($1), $3); $$.SetLocation(@$); }
  | "(" N ")"						{ $$ = $2; }
  ;



N : P1								{ $$ = $1; }
  ;

P1 : P1 "=" P2						{ $$ = new AssignmentExpression($1, $3); $$.SetLocation(@$); }
   | P2								{ $$ = $1; }
   ;

P2 : P2 "\|\|" P3					{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.OR, $1, $3); $$.SetLocation(@$); }
   | P3								{ $$ = $1; }
   ;

P3 : P3 "&&" P4						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.AND, $1, $3); $$.SetLocation(@$); }
   | P4								{ $$ = $1; }
   ;

P4 : P4 "==" P5						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.EQ, $1, $3); $$.SetLocation(@$); }
   | P4 "!=" P5						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.NOTEQ, $1, $3); $$.SetLocation(@$); }
   | P5								{ $$ = $1; }
   ;

P5 : P5 "<" P6						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.LESS, $1, $3); $$.SetLocation(@$); }
   | P5 ">" P6						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.MORE, $1, $3); $$.SetLocation(@$); }
   | P5 "<=" P6						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.LESSEQ, $1, $3); $$.SetLocation(@$); }
   | P5 ">=" P6						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.MOREEQ, $1, $3); $$.SetLocation(@$); }
   | P6								{ $$ = $1; }
   ;

P6 : P6 "+" P7						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.ADD, $1, $3); $$.SetLocation(@$); }
   | P6 "-" P7						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.MINUS, $1, $3); $$.SetLocation(@$); }
   | P7								{ $$ = $1; }
   ;

P7 : P7 "*" P8						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.MULTI, $1, $3); $$.SetLocation(@$); }
   | P7 "/" P8						{ $$ = new BinaryOperatorExpression(BinaryOperatorExpression.Operator.DIV, $1, $3); $$.SetLocation(@$); }
   | P8								{ $$ = $1; }
   ;

P8 : "-" P8							{ $$ = new UnaryOperatorExpression(UnaryOperatorExpression.Operator.MINUS, $2); $$.SetLocation(@$); }
   | "!" P8							{ $$ = new UnaryOperatorExpression(UnaryOperatorExpression.Operator.EXCLA, $2); $$.SetLocation(@$); }
   | E								{ $$ = $1; }
   ;

L : N								{ $$ = new List<Expression>(); $$.Add($1); }
  | L "," N							{ $$ = $1; $1.Add($3); }
  |									{ $$ = new List<Expression>(); }
  ;


%%

public Declaration program;
public Parser(Scanner s) : base(s) {}