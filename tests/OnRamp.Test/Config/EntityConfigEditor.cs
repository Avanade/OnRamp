using OnRamp.Config;
using System.Threading.Tasks;

namespace OnRamp.Test.Config
{
    public class EntityConfigEditor : IConfigEditor
    {
        public Task BeforePrepareAsync(IRootConfig config)
        {
            var ec = config as EntityConfig;
            ec.Name = ec.Name.ToUpperInvariant();
            return Task.CompletedTask;
        }
    }
}