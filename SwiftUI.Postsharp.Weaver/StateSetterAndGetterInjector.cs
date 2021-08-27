using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Extensibility;

namespace SwiftUI.Postsharp.Weaver
{
    public class StateSetterAndGetterInjector
    {
        private Project _project;

        public StateSetterAndGetterInjector(Project project)
        {
            _project = project;
        }

        public void Inject(PropertyDeclaration property)
        {
            var setter = property.Setter;
            var getter = property.Getter;
            ModifySetter(setter, property);
            ModifyGetter(getter, property);
        }

        private void ModifyGetter(MethodDefDeclaration getter, PropertyDeclaration property)
        {
            var targetType = property.DeclaringType;

            var backingFieldName = property.GetStateBackingField();
            var backingField = targetType.Fields.ToArray().First(x => x.Name.Equals(backingFieldName, StringComparison.Ordinal));

            var fieldType = backingField.FieldType.GetTypeDefinition();

            var backingFieldValueProperty = fieldType.Properties.First(x => x.Name.Equals("Value"));
            
            var valueGetter = (IGenericMethodDefinition)backingFieldValueProperty.Getter.Translate(_project.Module);
            var genericMap = new GenericMap(_project.Module, new List<ITypeSignature> { property.PropertyType });

            var genericValueGetter = valueGetter.GetGenericInstance(genericMap);

            var origin = getter.MethodBody.RootInstructionBlock;
            origin.Delete();

            var root = getter.MethodBody.CreateInstructionBlock();
            getter.MethodBody.RootInstructionBlock = root;

            var newSequence = root.AddInstructionSequence();

            using (var writer = InstructionWriter.GetInstance())
            {
                // Add instructions to the beginning of the method body:
                writer.AttachInstructionSequence(newSequence);

                // Say that what follows is compiler-generated code:
                writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

                writer.EmitInstruction(OpCodeNumber.Ldarg_0);
                writer.EmitInstructionField(OpCodeNumber.Ldfld, backingField);
                writer.EmitInstructionMethod(OpCodeNumber.Callvirt, genericValueGetter);
                writer.EmitInstruction(OpCodeNumber.Ret);

                writer.DetachInstructionSequence();
            }
        }

        private void ModifySetter(MethodDefDeclaration setter, PropertyDeclaration property)
        {
            var targetType = property.DeclaringType;

            var backingFieldName = property.GetStateBackingField();
            var backingField = targetType.Fields.ToArray().First(x => x.Name.Equals(backingFieldName, StringComparison.Ordinal));

            var fieldType = backingField.FieldType.GetTypeDefinition();

            var backingFieldValueProperty = fieldType.Properties.First(x => x.Name.Equals("Value"));

            var valueSetter = (IGenericMethodDefinition)backingFieldValueProperty.Setter.Translate(_project.Module);

            var genericMap = new GenericMap(_project.Module, new List<ITypeSignature> { property.PropertyType });

            var genericValueSetter = valueSetter.GetGenericInstance(genericMap);
            
            var origin = setter.MethodBody.RootInstructionBlock;
            origin.Delete();

            var root = setter.MethodBody.CreateInstructionBlock();
            setter.MethodBody.RootInstructionBlock = root;

            var newSequence = root.AddInstructionSequence();

            using (var writer = InstructionWriter.GetInstance())
            {
                // Add instructions to the beginning of the method body:
                writer.AttachInstructionSequence(newSequence);

                // Say that what follows is compiler-generated code:
                writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

                writer.EmitInstruction(OpCodeNumber.Ldarg_0);
                writer.EmitInstructionField(OpCodeNumber.Ldfld, backingField);
                writer.EmitInstruction(OpCodeNumber.Ldarg_1);
                writer.EmitInstructionMethod(OpCodeNumber.Callvirt, genericValueSetter);
                writer.EmitInstruction(OpCodeNumber.Ret);


                writer.DetachInstructionSequence();
            }
        }
    }
}
