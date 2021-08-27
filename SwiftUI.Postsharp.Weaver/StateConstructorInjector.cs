using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Extensibility;

namespace SwiftUI.Postsharp.Weaver
{
    public class StateConstructorInjector
    {
        private Project _project;

        public StateConstructorInjector(Project project)
        {
            _project = project;
        }

        public void Inject(TypeDefDeclaration targetType, PropertyDeclaration property)
        {
            var constructors = targetType.Methods.ToArray().Where(x => x.Name.Equals(".ctor"));

            foreach (var constructor in constructors)
            {
                InjectStateCreation(targetType, property, constructor);
            }
        }

        private void InjectStateCreation(TypeDefDeclaration targetType, PropertyDeclaration property, MethodDefDeclaration constructor)
        {
            var backingFieldName = property.GetStateBackingField();
            var backingField = targetType.Fields.ToArray().First(x => x.Name.Equals(backingFieldName, StringComparison.Ordinal));

            var backingFieldType = backingField.FieldType.GetTypeDefinition();

            var backingFieldTypeConstructor = (IGenericMethodDefinition)GetStateConstructor(backingFieldType).Translate(_project.Module);

            var genericMap = _project.Module.GetGenericMap(property.PropertyType);

            var genericConstructorRef = backingFieldTypeConstructor.GetGenericInstance(genericMap);

            var propertyVisitor = new PropertyInitReplacerVisitor(property) { StateConstructor = genericConstructorRef, StateFieldRef = backingField};
            constructor.MethodBody.Visit(new IMethodBodyVisitor[] { propertyVisitor }, MethodBodyVisitLevel.Instruction);

            if(!propertyVisitor.Rewrited)
            {
                var root = constructor.MethodBody.RootInstructionBlock;
                root.Detach();

                var newRoot = constructor.MethodBody.CreateInstructionBlock();

                constructor.MethodBody.RootInstructionBlock = newRoot;

                var initBlock = newRoot.AddChildBlock();

                var newSequence = initBlock.AddInstructionSequence();

                var localVarName = $"{property.GetStateBackingField()}_InitialValue";
                var localVar = newRoot.DefineLocalVariable(property.PropertyType, localVarName);

                using (var writer = InstructionWriter.GetInstance())
                {
                    // Add instructions to the beginning of the method body:
                    writer.AttachInstructionSequence(newSequence);

                    // Say that what follows is compiler-generated code:
                    writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

                    writer.EmitInstruction(OpCodeNumber.Ldarg_0);
                    writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloca_S, localVar);
                    writer.EmitInstructionType(OpCodeNumber.Initobj, property.PropertyType);
                    writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc_S, localVar);
                    writer.EmitInstructionMethod(OpCodeNumber.Newobj, genericConstructorRef);
                    writer.EmitInstructionField(OpCodeNumber.Stfld, backingField);

                    writer.DetachInstructionSequence();
                }

                newRoot.AddChildBlock(root);
            }
        }

        private bool IsSetFieldInstruction(Instruction x, PropertyDeclaration property)
        {
            if (x.OpCodeNumber != OpCodeNumber.Stfld)
            {
                return false;
            }

            if (x.OperandType != OperandType.InlineField)
            {
                return false;
            }

            if (!x.FieldOperand.Name.Equals(property.GetDefaultBackingFieldName()))
            {
                return false;
            }

            return true;
        }

        private MethodDefDeclaration GetStateConstructor(TypeDefDeclaration backingFieldType)
        {
            var constructors = backingFieldType.Methods.ToArray().Where(x => x.Name.Equals(".ctor"));

            return constructors.First(x => x.Attributes.HasFlag(MethodAttributes.Public));
        }
    }

    internal class PropertyInitReplacerVisitor : IMethodBodyVisitor
    {
        public bool Rewrited => !_tryReplace;

        public IMethod StateConstructor { get; set; }
        public FieldDefDeclaration StateFieldRef { get; set; }
        private PropertyDeclaration _targetProperty;

        private bool _tryReplace = true;

        public PropertyInitReplacerVisitor(PropertyDeclaration targetProperty)
        {
            _targetProperty = targetProperty;
        }

        public void EnterInstructionBlock(InstructionBlock instructionBlock)
        {
        }

        public void LeaveInstructionBlock(InstructionBlock instructionBlock)
        {
        }

        public void EnterInstructionSequence(InstructionSequence instructionSequence)
        {
        }

        public void LeaveInstructionSequence(InstructionSequence instructionSequence)
        {
        }

        public void VisitInstruction(InstructionReader instructionReader)
        {
            if (!_tryReplace)
            {
                return;
            }

            var instruction = instructionReader.CurrentInstruction;

            if (IsSetFieldInstruction(instruction, _targetProperty))
            {
                var currentSequence = instructionReader.CurrentInstructionSequence;

                var newSequence = instructionReader.CurrentInstructionBlock.AddInstructionSequence();

                instructionReader.EnterInstructionSequence(currentSequence);
                using (var writer = InstructionWriter.GetInstance())
                {
                    writer.AttachInstructionSequence(newSequence);

                    while (instructionReader.ReadInstruction() && instructionReader.CurrentInstructionSequence == currentSequence)
                    {
                        if (!IsSetFieldInstruction(instruction, _targetProperty))
                        {
                            instructionReader.CurrentInstruction.Write(writer);
                        }
                        else
                        {
                            writer.EmitInstructionMethod(OpCodeNumber.Newobj, StateConstructor);
                            writer.EmitInstructionField(OpCodeNumber.Stfld, StateFieldRef);
                        }
                    }
                    writer.DetachInstructionSequence();
                }

                currentSequence.Remove(newSequence);
                _tryReplace = false;
            }
        }

        private bool IsSetFieldInstruction(Instruction x, PropertyDeclaration property)
        {
            if (x.OpCodeNumber != OpCodeNumber.Stfld)
            {
                return false;
            }

            if (x.OperandType != OperandType.InlineField)
            {
                return false;
            }

            if (!x.FieldOperand.Name.Equals(property.GetDefaultBackingFieldName()))
            {
                return false;
            }

            return true;
        }

    }
}
