# SwiftUI.Postsharp

SwiftUI.Postsharp is addin for PostSharp

```
class Program
{
	[State] private int Count { get; set; } = 0;

	public string String => $"Count is: {Count}";

	static void Main(string[] args)
	{
	    Console.WriteLine("Hello World!");
	}
}
```

[Check on www.decompiler.com](https://www.decompiler.com)

```
private State<int> <StateWeaverGenerated>_Count = new State<int>(0);

private int Count
{
	get
	{
		return <StateWeaverGenerated>_Count;
	}
	set
	{
		<StateWeaverGenerated>_Count.Value  = value;
	}
}

public string String => $"Count is: {Count}";

private static void Main(string[] args)
{
	Console.WriteLine("Hello World!");
}


```

Simply install [Nuget package](https://www.nuget.org/packages/SwiftUI.Postsharp/1.0.1) in project with [Xamarin.SwiftUI package](https://github.com/chkn/Xamarin.SwiftUI)
