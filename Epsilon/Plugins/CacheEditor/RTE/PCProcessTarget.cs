using System.Diagnostics;
using System.IO;

namespace CacheEditor.RTE
{
    public class PCProcessTarget : IRteTarget
    {
        public PCProcessTarget(IRteProvider provider, Process process)
        {
            Provider = provider;
            Id = process.Id;
            DisplayName = $"{Path.GetFileName(process.MainModule.FileName)} #{process.Id}";
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
