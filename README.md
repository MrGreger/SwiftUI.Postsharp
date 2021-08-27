# SwiftUI.Postsharp

SwiftUI.Postsharp is addin for PostSharp

makes from

using System;
using SwiftUI;
using SwiftUI.Postsharp.Attributes;

namespace ConsoleApp8
{
    class Program
    {
        [State] private int Count { get; set; } = 0;

        public string String => $"Count is: {Count}";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}


using System;
using System.Runtime.CompilerServices;
using SwiftUI;

namespace ConsoleApp8
{
	internal class Program
	{
		private State<int> _003CStateWeaverGenerated_003E_Count = new State<int>(0);

		private int Count
		{
			[CompilerGenerated]
			get
			{
				return _003CStateWeaverGenerated_003E_Count.get_Value();
			}
			[CompilerGenerated]
			set
			{
				_003CStateWeaverGenerated_003E_Count.set_Value(value);
			}
		}

		public string String => $"Count is: {Count}";

		private static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}
	}
}
