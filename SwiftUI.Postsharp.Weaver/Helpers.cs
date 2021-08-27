using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Sdk.CodeModel;
using SwiftUI;


namespace SwiftUI.Postsharp.Weaver
{
    public static class Helpers
    {
        public static bool IsAutoProperty(this PropertyDeclaration property)
        {
            var backingFieldName = property.GetDefaultBackingFieldName();
            return property.DeclaringType.Fields.ToArray().Any(x => x.Name.Equals(backingFieldName));
        }

        public static string GetStateBackingField(this PropertyDeclaration property)
        {
            return $"<StateWeaverGenerated>_{property.Name}";
        }

        public static string GetDefaultBackingFieldName(this PropertyDeclaration property)
        {
            return $"<{property.Name}>k__BackingField";
        }

        public static ITypeSignature GetStateType(this ModuleDeclaration module)
        {
            return module.FindType(typeof(State<>));
        }

        public static GenericMap GetGenericMap(this ModuleDeclaration module, params ITypeSignature[] typeSignatures)
        {
            return new GenericMap(module, typeSignatures);
        }

    }

}
