using System.Linq;
using PostSharp.Sdk.CodeModel;

namespace SwiftUI.Postsharp.Weaver
{
    public class BackingFieldRemover
    {
        public void RemoveDefaultBackingField(TypeDefDeclaration typeDefinition, PropertyDeclaration propertyDefinition)
        {
            var fieldDefinition =
                typeDefinition.Fields.ToArray().First(x => x.Name.Equals(propertyDefinition.GetDefaultBackingFieldName()));
            typeDefinition.Fields.Remove(fieldDefinition);
        }
    }
}
