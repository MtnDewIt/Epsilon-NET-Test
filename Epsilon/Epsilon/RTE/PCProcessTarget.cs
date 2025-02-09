using System;
using System.Diagnostics;
using System.IO;
using Epsilon.RTE;
using Epsilon.Logging;

namespace Epsilon.RTE
{
	public class PCProcessTarget : IRteTarget, IEquatable<IRteTarget>
	{
		public int Id { get; }

		public IRteProvider Provider { get; }

		public string DisplayName { get; }

		object IRteTarget.Id => Id;

		private PCProcessTarget(IRteProvider provider, Process process) {
			Provider = provider;
			Id = process.Id;
			DisplayName = $"{Path.GetFileName(process.MainModule.FileName)} #{process.Id}";
		}

		public static bool TryCreate(IRteProvider provider, Process process, out PCProcessTarget target) {
			target = null;
			try {
				target = new PCProcessTarget(provider, process);
				return true;
			}
			catch (Exception arg) {
				Logger.Error($"RTE.PCProcessTarget.TryCreate threw an exception\n{arg}");
			}
			return false;
		}

		public bool Equals(IRteTarget other) {
			return Id.Equals(other.Id);
		}

		public override int GetHashCode() {
			return Id;
		}

		public override bool Equals(object obj) {
			return Equals((IRteTarget)obj);
		}
	}
}
