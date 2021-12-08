using OnRamp.Config;
using System.Threading.Tasks;

namespace OnRamp.Test.Config
{
    public class InheritAlternateConfigType : ConfigRootBase<InheritAlternateConfigType>
    {
        protected override Task PrepareAsync() => Task.CompletedTask;
    }
}