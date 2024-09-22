namespace Test1;

public class Program
{
    static void Main()
    {
        var @ref = TestObject.Get();
        var @ref2 = @ref;
        @ref.Put();
        
        Console.WriteLine(@ref2.IsNull());
        Console.WriteLine(@ref2.NotNull());
        
        List<TestObject.Ref> refs = new ();
        refs.Add(TestObject.Get());
        refs.Add(SubA.Get());
        refs.Add(SubB.Get());
        var subA = (SubA.Ref)refs[1];
        var subB = (SubB.Ref)refs[2];
        Console.WriteLine(subA.Object.InPool);
        Console.WriteLine(subB.Object.a);
        Console.WriteLine(subB.Object.b);

        //_TestFunc4<SubA>(null);
        //_TestFunc4(SubA.Get().Object);
        //_TestFunc2<SubA>();
        //var _TestFunc = _TestFunc2<SubA>().Pool;
        //_TestClass<SubA> _testClass = new _TestClass<SubA>(SubA.Get().Object);
        //_testClass.Set(SubA.Get().Object);
        //_testClass._TestVar = SubA.Get().Object;
        //_testClass.Func();
    }

    class _TestClass<T>
    {
        public _TestClass(object o)
        {
            _TestVar = (T)o;
        }

        public void Set(object o)
        {
            _TestVar = (T)o;
        }
        
        public T _TestVar;

        public T Func()
        {
            return _TestVar;
        }
    }
    
    public static T _TestFunc2<T>()
    {
        return default;
    }
    
    public static void _TestFunc4<T>(T a)
    {
        
    }
}