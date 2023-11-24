using Abdrakov.Fody;
using Fody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ModuleWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        var propertyWeaver = new AbdrakovPropertyWeaver
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
