using Microsoft.CodeAnalysis;

namespace PooledObjectAnalysis;

public static class Rules
{
	internal static readonly DiagnosticDescriptor NeedPartial = new(
		id: "PO1001",
		title: "Type must be partial",
		messageFormat: "Type '{0}' must be partial",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);


	internal static readonly DiagnosticDescriptor CtorNotAllowed = new(
		id: "PO1002",
		title: "Constructor is not allowed",
		messageFormat: "User defined constructors are not allowed for {0}",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);


	internal static readonly DiagnosticDescriptor AssignmentRule = new(
		id: "PO1003",
		title: "Do not allow any kind of assignment for PoolObject",
		messageFormat: "Do not allow any kind of assignment for PoolObject ({0}) in: {1}",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	
	
	internal static readonly DiagnosticDescriptor VariableRule = new(
		id: "PO1004",
		title: "Do not allow declare a PoolObject as a variable, field or property",
		messageFormat: "Do not allow declare a PoolObject ({0}) as a variable, field or property in: {1}",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	
	internal static readonly DiagnosticDescriptor MethodReturnRule = new(
		id: "PO1005",
		title: "Do not allow any method to return PoolObject",
		messageFormat: "Do not allow any method to return PoolObject ({0}) in: {1}",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	internal static readonly DiagnosticDescriptor MethodParamRule = new(
		id: "PO1006",
		title: "Do not allow any method has PoolObject as parameter",
		messageFormat: "Do not allow any method has PoolObject ({0}) as parameter in: {1}",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	
	internal static readonly DiagnosticDescriptor DelegateRule = new(
		id: "PO1007",
		title: "Do not allow any delegate has PoolObject as parameter or return type",
		messageFormat: "Do not allow any delegate has PoolObject ({0}) as parameter or return type in: {1}",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
	
	internal static readonly DiagnosticDescriptor CastRule = new(
		id: "PO1008",
		title: "Do not allow any cast a object from/to a PoolObject",
		messageFormat: "Do not allow any cast a object from/to a PoolObject ({0}) in: {1}",
		category: "",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true);
}