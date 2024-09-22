using System.Collections.Immutable;
using System.Diagnostics;
using cc_026.RoslynUtil;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PooledObjectAnalysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class PoolObjectAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rules.AssignmentRule, 
            Rules.VariableRule, 
            Rules.MethodReturnRule, 
            Rules.MethodParamRule,
            Rules.DelegateRule, 
            Rules.CastRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeSimpleAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
        context.RegisterSyntaxNodeAction(AnalyzeCoalesceAssignmentExpression, SyntaxKind.CoalesceAssignmentExpression);
        context.RegisterSyntaxNodeAction(AnalyzeEqualsValueClause, SyntaxKind.EqualsValueClause);
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclaration, SyntaxKind.VariableDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeCastExpression, SyntaxKind.CastExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAsExpression, SyntaxKind.AsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeIsPatternExpression, SyntaxKind.IsPatternExpression);
        context.RegisterSyntaxNodeAction(AnalyzeReturnStatement, SyntaxKind.ReturnStatement);
        context.RegisterSyntaxNodeAction(AnalyzeParenthesizedLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreationExpression, SyntaxKind.ObjectCreationExpression);

        //foreach (SyntaxKind value in Enum.GetValues(typeof(SyntaxKind)))
        //{
        //    context.RegisterSyntaxNodeAction(analysisContext => AnalyzeExpression(analysisContext, value), value);
        //}
    }

    //private void AnalyzeExpression(SyntaxNodeAnalysisContext context, SyntaxKind kind)
    //{
    //    var str = context.Node.ToFullString();
    //    if (str.Contains("_Test"))
    //    {
    //        //Debug.WriteLine($"{context.Node.GetType()}, {kind}\n{str}\n{new string('-', 100)}");
    //    }
    //}

    private static void AnalyzeSimpleAssignmentExpression(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        AnalyzeSyntaxNode(assignment, assignment.Left, context, Rules.AssignmentRule);
        AnalyzeSyntaxNode(assignment, assignment.Right, context, Rules.AssignmentRule);
    }

    private static void AnalyzeCoalesceAssignmentExpression(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        AnalyzeSyntaxNode(assignment, assignment.Left, context, Rules.AssignmentRule);
        AnalyzeSyntaxNode(assignment, assignment.Right, context, Rules.AssignmentRule);
    }

    private static void AnalyzeEqualsValueClause(SyntaxNodeAnalysisContext context)
    {
        var equal = (EqualsValueClauseSyntax)context.Node;
        AnalyzeSyntaxNode(equal, equal.Value, context, Rules.AssignmentRule);
    }

    private static void AnalyzeVariableDeclaration(SyntaxNodeAnalysisContext context)
    {
        var variable = (VariableDeclarationSyntax)context.Node;
        AnalyzeSyntaxNode(variable, variable.Type, context, Rules.VariableRule);
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var property = (PropertyDeclarationSyntax)context.Node;
        AnalyzeSyntaxNode(property, property.Type, context, Rules.VariableRule);
    }

    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var method = (MethodDeclarationSyntax)context.Node;
        AnalyzeSyntaxNode(method, method.ReturnType, context, Rules.MethodReturnRule);
        foreach (var parameter in method.ParameterList.Parameters)
        {
            AnalyzeSyntaxNode(method, parameter.Type, context, Rules.MethodParamRule);
        }
    }

    private static void AnalyzeCastExpression(SyntaxNodeAnalysisContext context)
    {
        var cast = (CastExpressionSyntax)context.Node;
        AnalyzeSyntaxNode(cast, cast.Expression, context, Rules.CastRule);
        AnalyzeSyntaxNode(cast, cast.Type, context, Rules.CastRule);
    }
    
    private static void AnalyzeAsExpression(SyntaxNodeAnalysisContext context)
    {
        var @as = (BinaryExpressionSyntax)context.Node;
        AnalyzeSyntaxNode(@as, @as.Left, context, Rules.CastRule);
        AnalyzeSyntaxNode(@as, @as.Right, context, Rules.CastRule);
    }
    
    private static void AnalyzeIsPatternExpression(SyntaxNodeAnalysisContext context) 
    { 
        var pattern = (IsPatternExpressionSyntax)context.Node;
        
        switch (pattern.Pattern) 
        { 
            // obj is null
            case ConstantPatternSyntax { Expression.RawKind: (int)SyntaxKind.NullLiteralExpression }:

            //obj is not null, we need roslyn 3.7.0 here for UnaryPatternSyntax type and SyntaxKind.NotPattern enum value
            case UnaryPatternSyntax { RawKind: (int)SyntaxKind.NotPattern, Pattern: ConstantPatternSyntax { Expression.RawKind: (int)SyntaxKind.NullLiteralExpression } }: 
                AnalyzeSyntaxNode(pattern, pattern.Expression, context, Rules.CastRule); 
                break; 
            
            case DeclarationPatternSyntax declarationPatternSyntax:
                AnalyzeSyntaxNode(pattern, pattern.Expression, context, Rules.CastRule); 
                AnalyzeSyntaxNode(pattern, declarationPatternSyntax.Type, context, Rules.CastRule); 
                break;
        } 
    }

    private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var @return = (ReturnStatementSyntax)context.Node;
        AnalyzeSyntaxNode(@return, @return.Expression, context, Rules.MethodReturnRule);
    }

    private static void AnalyzeParenthesizedLambdaExpression(SyntaxNodeAnalysisContext context)
    {
        var lambda = (ParenthesizedLambdaExpressionSyntax)context.Node;
        AnalyzeSyntaxNode(lambda, lambda.ExpressionBody, context, Rules.MethodReturnRule);
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        switch (invocation.Expression)
        {
            case GenericNameSyntax genericNameSyntax:
                foreach (var arg in genericNameSyntax.TypeArgumentList.Arguments)
                {
                    AnalyzeSyntaxNode(invocation, arg, context, Rules.MethodParamRule);
                }
                break;
            default:
                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    AnalyzeSyntaxNode(invocation, arg.Expression, context, Rules.MethodParamRule);
                }
                break;
        }

        AnalyzeSyntaxNode(invocation, invocation.Expression, context, Rules.MethodReturnRule);
    }
    
    private static void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
    {
        var creation = (ObjectCreationExpressionSyntax)context.Node;
        if(null == creation.ArgumentList) return;
        foreach (var argument in creation.ArgumentList.Arguments)
        {
            AnalyzeSyntaxNode(creation, argument.Expression, context, Rules.MethodParamRule);
        }
    }
    
    private static void AnalyzeSyntaxNode(SyntaxNode originalExpression, ExpressionSyntax? typedExpression,
        SyntaxNodeAnalysisContext context, DiagnosticDescriptor rule)
    {
        if(null == typedExpression)
            return;
        
        var type = context.SemanticModel.GetTypeInfo(typedExpression);
        var symbol = context.SemanticModel.GetSymbolInfo(typedExpression).Symbol;
        //Debug.WriteLine($"{new string('-', 100)}\n TYPE: {type.Type}, {symbol}\n SType: {type.Type?.GetType()}, {symbol?.GetType()} \n Syntax: {typedExpression.ToFullString()}\n {new string('-', 100)}");
        switch (symbol)
        {
            case IMethodSymbol methodSymbol:
                AnalyzeSyntaxNode(originalExpression, methodSymbol.ReturnType, context, Rules.MethodReturnRule);
                foreach (var param in methodSymbol.Parameters)
                {
                    AnalyzeSyntaxNode(originalExpression, param?.Type, context, Rules.MethodParamRule);
                }

                break;
        }
        AnalyzeSyntaxNode(originalExpression, type.Type, context, rule);
    }

    private static void AnalyzeSyntaxNode(SyntaxNode originalExpression, ITypeSymbol? typeSymbol,
        SyntaxNodeAnalysisContext context, DiagnosticDescriptor rule)
    {
        if (null == typeSymbol)
            return;

        if (typeSymbol.Extends(Const.PoolObjectType))
        {
            Debug.WriteLine($"ERROR {originalExpression.ToFullString()}");
            context.ReportDiagnostic(Diagnostic.Create(rule, originalExpression.GetLocation(),
                typeSymbol.MiniName(), originalExpression.ToFullString()));
        }

        if (TypeKind.Delegate == typeSymbol.TypeKind)
        {
            var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;
            var delegateInvokeMethod = namedTypeSymbol?.DelegateInvokeMethod;
            if (null != delegateInvokeMethod)
            {
                AnalyzeSyntaxNode(originalExpression, delegateInvokeMethod.ReturnType, context, Rules.DelegateRule);
                foreach (var parameterSymbol in delegateInvokeMethod.Parameters)
                {
                    AnalyzeSyntaxNode(originalExpression, parameterSymbol?.Type, context, Rules.DelegateRule);
                }
            }
        }
    }
}