using Hypocrite.Fody.AttributeWeavers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Linq;

namespace Hypocrite.Fody
{
    public class HypocritePropertyWeaver
    {
        /// <summary>
        /// Gets or sets the module definition.
        /// </summary>
        /// <value>
        /// The module definition.
        /// </value>
        public ModuleDefinition ModuleDefinition { get; set; }

        /// <summary>
        /// Gets or sets a action that will log an MessageImportance.High message to MSBuild. OPTIONAL.
        /// </summary>
        /// <value>
        /// The log information.
        /// </value>
        public Action<string> LogInfo { get; set; }

        /// <summary>
        /// Gets or sets an action that will log an error message to MSBuild. OPTIONAL.
        /// </summary>
        /// <value>
        /// The log error.
        /// </value>
        public Action<string> LogError { get; set; }

        /// <summary>
        /// Executes this property weaver.
        /// </summary>
        /// <exception cref="Exception">
        /// reactiveObjectExtensions is null
        /// or
        /// raiseAndSetIfChangedMethod is null
        /// or
        /// reactiveAttribute is null
        /// or
        /// [Reactive] is decorating " + property.DeclaringType.FullName + "." + property.Name + ", but the property has no setter so there would be nothing to react to.  Consider removing the attribute.
        /// </exception>
        public void Execute()
        {
            if (ModuleDefinition is null)
            {
                LogInfo?.Invoke("The module definition has not been defined.");
                return;
            }

            var engine = ModuleDefinition.AssemblyReferences.Where(x => x.Name == "Hypocrite.Core").OrderByDescending(x => x.Version).FirstOrDefault();
            if (engine is null)
            {
                LogInfo?.Invoke("Could not find assembly: Hypocrite.Core (" + string.Join(", ", ModuleDefinition.AssemblyReferences.Select(x => x.Name)) + ")");
                return;
            }

            LogInfo?.Invoke($"{engine.Name} {engine.Version}");
            var bindableObject = new TypeReference("Hypocrite.Core.Mvvm", "BindableObject", ModuleDefinition, engine);
            var targetTypes = ModuleDefinition.GetAllTypes().Where(x => x.BaseType != null && bindableObject.IsAssignableFrom(x.BaseType)).ToArray();
            var bindableObjectExtensions = new TypeReference("Hypocrite.Core.Extensions", "BindableObjectExtensions", ModuleDefinition, engine).Resolve() ?? throw new Exception("BindableObjectExtensions is null");
            var raiseAndSetPropertyMethod = ModuleDefinition.ImportReference(bindableObjectExtensions.Methods.Single(x => x.Name == "RaiseAndSetIfChanged")) ?? throw new Exception("RaiseAndSetIfChanged is null");
            var notifyAttribute = ModuleDefinition.FindType("Hypocrite.Core.Mvvm.Attributes", "NotifyAttribute", engine) ?? throw new Exception("NotifyAttribute is null");
            foreach (var targetType in targetTypes)
            {
                // search for [Notify]
                foreach (var property in targetType.Properties.Where(x => x.IsDefined(notifyAttribute)).ToArray())
                {
                    NotifyWeaver.Weave(property, LogError, targetType, raiseAndSetPropertyMethod);
				}
            }
        }
    }
}
