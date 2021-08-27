using System;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Tasks;
using SwiftUI.Postsharp.Attributes;

namespace SwiftUI.Postsharp.Weaver
{    
    // The [RequirePostSharp] attribute on HelloWorldAttribute causes PostSharp to look through assemblies on its search path
    // for assemblies named PostSharp.Community.HelloWorld.Weaver.dll which contain an exported task named HelloWorldTask.
    // By default, custom tasks are executed after all other transformations. This can behavior can be overwritten by setting the Phase
    // property and by using the [TaskDependency] attribute.
    [ExportTask(Phase = TaskPhase.CustomTransform, TaskName = nameof(WeaveStatesTask))]
    public class WeaveStatesTask : Task
    {// Services imported this way are injected during task construction and can be used during Execute:
        [ImportService]
        private IAnnotationRepositoryService _annotationRepositoryService;

        // This string, if defined, is printed to standard output if you build a project that uses this add-in from commandline.
        // It will not show up in Visual Studio/Rider.
        public override string CopyrightNotice => null;

        public override bool Execute()
        {
            var enumerator =
                _annotationRepositoryService.GetAnnotationsOfType(typeof(StateAttribute), false, false);
            while (enumerator.MoveNext())
            {
                // Iterates over declarations to which our attribute has been applied. If the attribute weren't
                // a MulticastAttribute, that would be just the declarations that it annotates. With multicasting, it 
                // can be far more declarations.

                MetadataDeclaration targetDeclaration = enumerator.Current.TargetElement;

                // Multicasting ensures that our attribute is only applied to methods, so there is little chance of 
                // a class cast error here:
                PropertyDeclaration targetProperty = (PropertyDeclaration)targetDeclaration;

                var targetType = targetProperty.DeclaringType;

                if (!targetProperty.IsAutoProperty())
                {
                    continue;
                }

                new StateFieldInjector(Project).Inject(targetType, targetProperty);
                new StateSetterAndGetterInjector(Project).Inject(targetProperty);
                new StateConstructorInjector(Project).Inject(targetType, targetProperty);
                new BackingFieldRemover().RemoveDefaultBackingField(targetType, targetProperty);
            }

            return true;
        }
    }
}
