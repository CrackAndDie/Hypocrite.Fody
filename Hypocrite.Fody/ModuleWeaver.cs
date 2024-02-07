using Hypocrite.Fody;
using Fody;
using System.Collections.Generic;

public class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        var propertyWeaver = new HypocritePropertyWeaver
        {
            ModuleDefinition = ModuleDefinition,
            LogInfo = WriteInfo,
            LogError = WriteError
        };
        propertyWeaver.Execute();
    }

    /// <inheritdoc/>
    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "netstandard";
        yield return "System";
        yield return "System.Runtime";
        yield return "Abdrakov.Engine";
        yield return "Abdrakov.CommonWPF";
    }

    public override bool ShouldCleanReference => true;
}
