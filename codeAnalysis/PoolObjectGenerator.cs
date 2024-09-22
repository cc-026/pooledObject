using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using cc_026.Pool;
using cc_026.RoslynUtil;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace PooledObjectAnalysis;

[Generator(LanguageNames.CSharp)]
public class PoolObjectGenerator : IIncrementalGenerator
{
    struct CodeTemplate
    {
        internal string PoolCode;
        internal string RefCode;
        internal string RefCastCode;
        //internal string RefFieldCode;
    }
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider
            .CreateSyntaxProvider((node, _) => node is ClassDeclarationSyntax,
                (ctx, token) => (ctx, token).GetSymbol<ITypeSymbol>())
            .Where(typeSymbol => typeSymbol?.Extends(Const.PoolObjectType) ?? false)
            .Collect()
            .SelectMany((array, _) => array.Distinct());
            //.Combine(context.CompilationProvider);

        var codeTemplate = new CodeTemplate()
        {
            PoolCode = EmbeddedResource.GetContent(Const.PoolCode),
            RefCode = EmbeddedResource.GetContent(Const.RefCode),
            RefCastCode = EmbeddedResource.GetContent(Const.RefCastCode),
            //RefFieldCode = EmbeddedResource.GetContent(Const.RefFieldCode),
        };
        
        context.RegisterSourceOutput(source, (productionContext, typeSymbol) => DoGen(productionContext, typeSymbol!, codeTemplate));
    }

    private void DoGen(SourceProductionContext productionContext, ITypeSymbol typeSymbol, CodeTemplate codeTemplate)
    {
        var fullName = typeSymbol.FullName();
        var miniName = typeSymbol.MiniName();
        var fileName = typeSymbol.FileName();
        Debug.WriteLine($"Type is {fullName}");
        
        if (false == typeSymbol.IsPartial())
        {
            foreach (var location in typeSymbol.Locations)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(Rules.NeedPartial, location,
                    miniName));
            }
            return;
        }

        var ctor = typeSymbol.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .SelectMany(node =>
                node.ChildNodes().Where(syntaxNode => syntaxNode.IsKind(SyntaxKind.ConstructorDeclaration))).ToList();
        if (ctor.Count > 0)
        {
            foreach (var c in ctor)
            {
                productionContext.ReportDiagnostic(Diagnostic.Create(Rules.CtorNotAllowed, c.GetLocation(),
                    miniName));
            }

            return;
        }

        var poolSettings = Pool.PoolSettings.DefaultValue;
        PoolSetting(typeSymbol.ContainingAssembly.GetAttributes(), ref poolSettings);
        PoolSetting(typeSymbol.GetAttributes(), ref poolSettings);
        
        var poolSettingCodeBuilder = new StringBuilder();
        var settingType = poolSettings.GetType();
        poolSettingCodeBuilder.AppendLine($"new {settingType.Name}");
        poolSettingCodeBuilder.AppendLine("{");
        foreach (var field in settingType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var value = field.GetValue(poolSettings);
            var valueStr = value.ToString();
            valueStr = value switch
            {
                bool => valueStr.ToLower(),
                _ => valueStr
            };
            poolSettingCodeBuilder.AppendLine($"{field.Name} = {valueStr},");
        }
        poolSettingCodeBuilder.AppendLine("}");

        Debug.WriteLine(poolSettingCodeBuilder);

        string poolCode, refCode;
        if (typeSymbol.IsAbstract)
        {
            // https://blog.csdn.net/weixin_43788143/article/details/111871973
            // https://blog.csdn.net/qq_38240227/article/details/118993412
            poolCode = Regex.Replace(codeTemplate.PoolCode, @$"({Const.AbstractPlaceholder})[\s\S]*?({Const.AbstractPlaceholder})",
                string.Empty);
            refCode = Regex.Replace(codeTemplate.RefCode, @$"({Const.AbstractPlaceholder})[\s\S]*?({Const.AbstractPlaceholder})",
                string.Empty);
        }
        else
        {
            poolCode = codeTemplate.PoolCode
                .Replace(Const.AbstractPlaceholder, string.Empty);
            refCode = codeTemplate.RefCode
                .Replace(Const.AbstractPlaceholder, string.Empty);
        }

        poolCode = poolCode
            .Replace(Const.ClassPlaceholder, miniName);
        refCode = refCode
            .Replace(Const.ClassPlaceholder, miniName);
        var ns = typeSymbol.ContainingNamespace;
        if (false == ns.IsGlobalNamespace)
        {
            poolCode = poolCode
                .Replace(Const.NamespaceStartPlaceholder, $"namespace {ns}\n {{")
                .Replace(Const.NamespaceEndPlaceholder, "}");
            refCode = refCode
                .Replace(Const.NamespaceStartPlaceholder, $"namespace {ns}\n {{")
                .Replace(Const.NamespaceEndPlaceholder, "}");
        }
        else
        {
            poolCode = poolCode
                .Replace(Const.NamespaceStartPlaceholder, string.Empty)
                .Replace(Const.NamespaceEndPlaceholder, string.Empty);
            refCode = refCode
                .Replace(Const.NamespaceStartPlaceholder, string.Empty)
                .Replace(Const.NamespaceEndPlaceholder, string.Empty);
        }

        var castCodeBuilder = new StringBuilder();
        var detailCodeBuilder = new StringBuilder();
        GenerateCast(typeSymbol.BaseType, castCodeBuilder, codeTemplate.RefCastCode);
        //GenerateDetail(typeDefs[0].TypeData, detailCodeBuilder, codeTemplate);
        poolCode = poolCode
            .Replace(Const.PoolSettingPlaceholder, poolSettingCodeBuilder.ToString());
        refCode = refCode
            .Replace(Const.RefCastPlaceholder, castCodeBuilder.ToString())
            .Replace(Const.DetailPlaceholder, detailCodeBuilder.ToString());
        
        poolCode = CSharpSyntaxTree.ParseText(poolCode).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
        refCode = CSharpSyntaxTree.ParseText(refCode).GetRoot().NormalizeWhitespace().SyntaxTree.GetText().ToString();
        Debug.WriteLine(poolCode);
        Debug.WriteLine(refCode);
        productionContext.AddSource($"{fileName}.Pool.g.cs", poolCode);
        productionContext.AddSource($"{fileName}.Ref.g.cs", refCode);
    }

    private void GenerateCast(ITypeSymbol? typeSymbol, StringBuilder castCodeBuilder, string refCastCode)
    {
        if (null == typeSymbol || typeSymbol.Matches(Const.PoolObjectType))
            return;

        castCodeBuilder.AppendLine(refCastCode.Replace(Const.BaseClassPlaceholder, typeSymbol.FullName()));

        GenerateCast(typeSymbol.BaseType, castCodeBuilder, refCastCode);
    }

    //private void GenerateDetail(TypeData typeData, StringBuilder detailBuilder, CodeTemplate codeTemplate)
    //{
    //    foreach (var f in typeData.AccessibleFields.OrderBy(p => p.Key))
    //    {
    //        var field = f.Value;
    //        if (field.IsStatic || field.IsConst)
    //            continue;
    //        
    //        var fieldCode = field.IsReadOnly
    //            ? Regex.Replace(codeTemplate.RefFieldCode, 
    //                @$"({Const.SetPlaceholder})[\s\S]*?({Const.SetPlaceholder})",
    //                string.Empty)
    //            : codeTemplate.RefFieldCode;

    //        fieldCode = fieldCode
    //            .Replace(Const.TypeNamePlaceholder, field.Type.FullName())
    //            .Replace(Const.FieldNamePlaceholder, field.FullName())
    //            .Replace(Const.AccessModifierPlaceholder, GetAccessModifier(field.DeclaredAccessibility).ToString());
    //        detailBuilder.AppendLine(fieldCode);
    //    }
    //    
    //    EAccessModifier GetAccessModifier(Accessibility accessibility) =>
    //        accessibility switch
    //        {
    //            Accessibility.Private => EAccessModifier.Private,
    //            Accessibility.ProtectedAndInternal => EAccessModifier.PrivateProtected,
    //            Accessibility.Protected => EAccessModifier.Protected,
    //            Accessibility.Internal => EAccessModifier.Internal,
    //            Accessibility.ProtectedOrInternal => EAccessModifier.ProtectedInternal,
    //            _ => EAccessModifier.Public 
    //        };
    //}

    private void PoolSetting(ImmutableArray<AttributeData> array, ref Pool.PoolSettings poolSettings)
    {
        foreach (var attribute in array)
        {
            if (attribute.AttributeClass.Matches(Const.PoolInitCapacityType))
            {
                poolSettings.InitCapacity = (int)attribute.ConstructorArguments[0].Value!;
            }

            if (attribute.AttributeClass.Matches(Const.PoolIsAsynchronousSafeType))
            {
                poolSettings.IsAsynchronousSafe = (bool)attribute.ConstructorArguments[0].Value!;
            }
        }
    }
}