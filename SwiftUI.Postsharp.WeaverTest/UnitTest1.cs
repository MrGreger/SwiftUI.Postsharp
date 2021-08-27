using SwiftUI.Postsharp.Attributes;
using System;
using Xunit;

namespace SwiftUI.Postsharp.WeaverTest
{
    public class UnitTest1
    {

        [Fact]
        public void Test1()
        {
            var testModel = new TestModel();
        }
    }

    public class TestModel
    {
        [State] public int Int { get; set; } = 12;
        [State] public int Int2 { get; set; } = 13;
        [State] public int Int3 { get; set; } = 13;
        [State] public int Int4 { get; set; } = 13;
        [State] public int Int5 { get; set; }
        [State] public int Int6 { get; set; }

        [State] public string Str { get; set; }
        [State] public string Str2 { get; set; } = "SADf";

        [State] public DateTime Date { get; set; }
        [State] public DateTime Date2 { get; set; } = DateTime.Now.AddDays(-1);

        //public int NotAutoProp => 1;
    }
}
