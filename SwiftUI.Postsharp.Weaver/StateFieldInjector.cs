using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Extensibility;

namespace SwiftUI.Postsharp.Weaver
{
    public class StateFieldInjector
    {
        private Project _project;

        public StateFieldInjector(Project project)
        {
            _project = project;
        }

        public void Inject(TypeDefDeclaration targetType, PropertyDeclaration property)
        {
            InjectStateField(targetType, property);
        }

        private void InjectStateField(TypeDefDeclaration targetType, PropertyDeclaration property)
        {
            var stateType = (IGenericTypeDefinition)_project.Module.GetStateType();

            var genericMap = new GenericMap(_project.Module, new List<ITypeSignature> { property.PropertyType });
            
            var stateFieldDefinition = new FieldDefDeclaration()
            {
                Attributes = FieldAttributes.Private,
                Name = property.GetStateBackingField(),
                FieldType = stateType.GetGenericInstance(genericMap)
            };

            targetType.Fields.Add(stateFieldDefinition);
        }
    }
}
