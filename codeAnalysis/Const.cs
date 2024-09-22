using System;
using cc_026.Pool;

namespace PooledObjectAnalysis;

public class Const
{
    //internal const string BaseNameSpace = "cc_026.Pool";
    //internal const string PoolClassName = "PoolObject";
    //internal const string PoolClassFullName = $"{BaseNameSpace}.{PoolClassName}";
    internal static readonly Type PoolObjectType = typeof(PoolObject);
    internal static readonly Type PoolInitCapacityType = typeof(InitCapacity);
    internal static readonly Type PoolIsAsynchronousSafeType = typeof(IsAsynchronousSafe);
    internal const string PoolCode = "TemplatePoolCode.t2";
    internal const string RefCode = "TemplateRefCode.t2";
    internal const string RefCastCode = "TemplateRefCastCode.t2";
    internal const string RefFieldCode = "TemplateRefFieldCode.t2";
    internal const string ClassPlaceholder = "__CLASSNAME__";
    internal const string DetailPlaceholder = "__DETAIL__";
    internal const string BaseClassPlaceholder = "__BASECLASS__";
    internal const string RefCastPlaceholder = "__REFAUTOCAST__";
    internal const string NamespaceStartPlaceholder = "__NAMESPACESTART__";
    internal const string NamespaceEndPlaceholder = "__NAMESPACEEND__";
    internal const string AbstractPlaceholder = "__ABSTRACT__";
    internal const string TypeNamePlaceholder = "__TYPENAME__";
    internal const string PoolSettingPlaceholder = "__POOLSETTING__";
    internal const string AccessModifierPlaceholder = "__ACCESSMODIFIER__";
    internal const string FieldNamePlaceholder = "__FIELDNAME__";
    internal const string SetPlaceholder = "//__SET__";
}
