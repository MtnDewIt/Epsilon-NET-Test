using EpsilonLib.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace CacheEditor.RTE
{
    public class PCProcessTarget : IRteTarget
    {
        private PCProcessTarget(IRteProvider provider, Process process)
        {
            Provider = provider;
            Id = process.Id;
            DisplayName = $"{Path.GetFileName(process.MainModule.FileName)} #{process.Id}";
        }

        public static bool TryCreate(IRteProvider provider, Process process, out PCProcessTarget target)
        {
            target = null;

            try
            {
                target = new PCProcessTarget(provider, process);
                return true;
            }
            catch(Exception ex)
            {
                Logger.Error($"RTE.PCProcessTarget.TryCreate threw an exception\n{ex}");
            }

            return false;
        }

        public int Id { get; }
        public IRteProvider Provider { get; }
        public string DisplayName { get; }

        object IRteTarget.Id => Id;

        public bool Equals(IRteTarget other) => Id.Equals(other.Id);

        public override int GetHashCode() => Id;

        public override bool Equals(object obj) => Equals((IRteTarget)obj);
    }
}
