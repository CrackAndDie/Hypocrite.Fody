using Hypocrite.Fody.AttributeWeavers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
            
            // for notify
            var raiseAndSetPropertyMethod = ModuleDefinition.ImportReference(bindableObjectExtensions.Methods.Single(x => x.Name == "RaiseAndSetIfChanged")) ?? throw new Exception("RaiseAndSetIfChanged is null");
            var notifyAttribute = ModuleDefinition.FindType("Hypocrite.Core.Mvvm.Attributes", "NotifyAttribute", engine) ?? throw new Exception("NotifyAttribute is null");

            // for notifyWhen
            var bindableObjectResolved = bindableObject.Resolve();
			var raisePropertyMethod = ModuleDefinition.ImportReference(bindableObjectResolved.Methods.Single(x => x.Name == "RaisePropertyChanged")) ?? throw new Exception("RaisePropertyChanged is null");
			var notifyWhenAttribute = ModuleDefinition.FindType("Hypocrite.Core.Mvvm.Attributes", "NotifyWhenAttribute", engine) ?? throw new Exception("NotifyWhenAttribute is null. Probably an old version on Hypocrite.Services is used");
			var alsoNotifyAttribute = ModuleDefinition.FindType("Hypocrite.Core.Mvvm.Attributes", "AlsoNotifyAttribute", engine) ?? throw new Exception("AlsoNotifyAttribute is null. Probably an old version on Hypocrite.Services is used");
			foreach (var targetType in targetTypes)
            {
                var allProps = targetType.Properties;
                Dictionary<string, List<string>> notifyToDeps = new Dictionary<string, List<string>>();

                foreach (var property in allProps)
                {
                    // prepare props with [NotifyWhen(...)] attrs
					var whenAttrs = property.CustomAttributes.GetAttributes("Hypocrite.Core.Mvvm.Attributes.NotifyWhenAttribute");
					foreach (var attr in whenAttrs)
					{
						var ctrArgs = attr.ConstructorArguments;
						PrepareDict(notifyToDeps, (string)ctrArgs[0].Value, property.Name);

                        // if there is only one arg passed to the constructor
                        if (ctrArgs.Count <= 1)
                            continue;

						var otherValue = (CustomAttributeArgument[])ctrArgs[1].Value;
						foreach (string other in otherValue.Select(_ => _.Value))
						{
							PrepareDict(notifyToDeps, other, property.Name);
						}
					}
					// prepare props with [AlsoNotify(...)] attrs
					var alsoAttrs = property.CustomAttributes.GetAttributes("Hypocrite.Core.Mvvm.Attributes.AlsoNotifyAttribute");
					foreach (var attr in alsoAttrs)
					{
						var ctrArgs = attr.ConstructorArguments;
						PrepareDict(notifyToDeps, property.Name, (string)ctrArgs[0].Value);

						// if there is only one arg passed to the constructor
						if (ctrArgs.Count <= 1)
							continue;

						var otherValue = (CustomAttributeArgument[])ctrArgs[1].Value;
						foreach (string other in otherValue.Select(_ => _.Value))
						{
							PrepareDict(notifyToDeps, property.Name, other);
						}
					}
				}

				// search for [Notify]
				foreach (var property in allProps.Where(x => x.IsDefined(notifyAttribute)))
                {
                    // getting notify deps
                    List<string> deps = new List<string>();
                    if (notifyToDeps.ContainsKey(property.Name))
						deps = notifyToDeps[property.Name];

                    // weave all
					NotifyWeaver.Weave(property, LogError, targetType, raiseAndSetPropertyMethod, raisePropertyMethod, deps);
				}
            }
        }

        private static void PrepareDict(Dictionary<string, List<string>> dict, string key, string newValue)
        {
            if (dict.ContainsKey(key))
            {
                if (dict[key] == null)
                    dict[key] = new List<string>();

                dict[key].Add(newValue);
            }
            else
            {
				dict[key] = new List<string>
				{
					newValue
				};
			}
        }
    }
}
