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

            var p = ec.Properties[2];
            if (p.Count != 4 || p.Amount != 3.95M)
                throw new System.InvalidOperationException("Count or Amount was incorrectly parsed.");

            return Task.CompletedTask;
        }
    }
}