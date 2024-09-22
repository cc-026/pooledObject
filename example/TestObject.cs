using cc_026.Pool;

namespace Test1;

public abstract partial class TestAbstractObject : PoolObject
{
}
public partial class SubC : TestAbstractObject
{
    protected override void OnGet(object param)
    {
    }

    protected override void OnPut()
    {
    }
}

public partial class TestObject : PoolObject
{
    protected override void OnGet(object param)
    {
        Console.WriteLine("TestObject OnGet");
    }

    protected override void OnPut()
    {
        Console.WriteLine("TestObject OnPut");
    }

    //public int publicField;
    //protected int protectedField;
    //internal int internamField;
    //protected internal int protectedInternalField;
    //private int privateField;
    //private protected int privateProtectedField;
    //
    //public int publicFunc(){ return 0 ;}
    //protected int protectedFunc(){ return 0 ;}
    //internal int internamFunc(){ return 0 ;}
    //protected internal int protectedInternalFunc(){ return 0 ;}
    //private int privateFunc(){ return 0 ;}
    //private protected int privateProtectedFunc(){ return 0 ;}
}

[InitCapacity(2)]
public partial class SubA : TestObject
{
    protected override void OnCtor()
    {
        base.OnCtor();
        Console.WriteLine("SubA OnCtor");
    }

    protected override void OnGet(object param)
    {
        base.OnGet(param);
        Console.WriteLine("SubA OnGet");
    }

    protected override void OnPut()
    {
        base.OnPut();
        Console.WriteLine("SubA OnPut");
    }

    public override bool Equals(object? obj)
    {
        Console.WriteLine("SubA Equals");
        return base.Equals(obj);
    }

    public override string ToString()
    {
        Console.WriteLine("SubA ToString");
        return base.ToString();
    }

    public override int GetHashCode()
    {
        Console.WriteLine("SubA GetHashCode");
        return base.GetHashCode();
    }
}

public partial class SubB : SubA
{
    public int a;
}

public partial class SubB
{
    public int b;
}