using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using TagTool.Cache;
using TagTool.Cache.HaloOnline;
using TagTool.Commands.Editing;
using TagTool.IO;
using TagTool.Serialization;

namespace Epsilon.RTE
{
	[Export(typeof(IRteProvider))]
	internal class HaloOnline106708Provider : IRteProvider, IRteTargetSource
	{
		public IEnumerable<IRteTarget> FindTargets() {
			Process[] processesByName = Process.GetProcessesByName("eldorado");
			foreach (Process process in processesByName) {
				if (PCProcessTarget.TryCreate(this, process, out var target)) {
					yield return target;
				}
				target = null;
			}
		}

		public bool ValidForCacheFile(ICacheFile cacheFile) {
			return cacheFile.Cache is GameCacheHaloOnlineBase;
		}

		void IRteProvider.PokeTag(IRteTarget target, GameCache cache, CachedTag instance, object definition, ref byte[] RuntimeTagData) {
			PokeTag(target, cache, definition, instance as CachedTagHaloOnline, ref RuntimeTagData);
		}

		private void PokeTag(IRteTarget target, GameCache cache, object definition, CachedTagHaloOnline hoInstance, ref byte[] RuntimeTagDataMap) {
			Process process = Process.GetProcessById((int)target.Id);
			if (process == null) {
				throw new RteTargetNotAvailableException(this, "Target process could not be found.");
			}
			using (ProcessMemoryStream processStream = new ProcessMemoryStream(process)) {
				int tagindex = hoInstance.Index;
				bool isModPackage = false;
				GameCacheModPackage modpak = null;
				if (cache is GameCacheModPackage) {
					modpak = (GameCacheModPackage)cache;
					isModPackage = true;
				}
				tagindex = ResolveTagIndex(tagindex, isModPackage, modpak);
				uint address = GetTagAddress(processStream, tagindex);
				if (address == 0) {
					throw new RteProviderException(this, $"Tag '{hoInstance}' could not be located in the target process.");
				}
				byte[] tagcachedata;
				using (Stream stream = cache.OpenCacheRead()) {
					using (MemoryStream outstream = new MemoryStream()){
						using (EndianWriter writer = new EndianWriter(outstream)) {
							object cachedef = cache.Deserialize(stream, hoInstance);
							DataSerializationContext dataContext = new DataSerializationContext(writer);
							cache.Serializer.Serialize(dataContext, cachedef);
							StreamUtil.Align(outstream, 16);
							tagcachedata = outstream.ToArray();
						}
					}
				}
				byte[] editordata;
				using (MemoryStream stream2 = new MemoryStream()) {
					using (EndianWriter writer2 = new EndianWriter(stream2)){
						DataSerializationContext dataContext2 = new DataSerializationContext(writer2);
						cache.Serializer.Serialize(dataContext2, definition);
						StreamUtil.Align(stream2, 16);
						editordata = stream2.ToArray();
					}
				}
				if (tagcachedata.Length != editordata.Length) {
					throw new RteProviderException(this, "Error: tag size changed or the serializer failed!");
				}
				if (!isModPackage && tagcachedata.Length != hoInstance.TotalSize - hoInstance.CalculateHeaderSize()) {
					throw new RteProviderException(this, "Sorry can't poke this specific tag yet (only happens with very rare specific tags), go bug a dev");
				}
				Stopwatch stopWatch = new Stopwatch();
				stopWatch.Start();
				process.Suspend();
				uint currenttotalsize = 0u;
				uint headersize = 0u;
				List<uint> TagReferenceFixups = new List<uint>();
				using (EndianReader reader = new EndianReader(processStream)) {
					CachedTagHaloOnline runtimetag = new CachedTagHaloOnline();
					processStream.Seek(address + 4, SeekOrigin.Begin);
					ReadHeaderValues(reader, ref currenttotalsize, ref headersize, ref TagReferenceFixups);
				}
				byte[] CurrentRuntimeTagData = new byte[currenttotalsize - headersize];
				processStream.Seek(address + headersize, SeekOrigin.Begin);
				processStream.Read(CurrentRuntimeTagData, 0, (int)( currenttotalsize - headersize ));
				if (currenttotalsize - headersize != tagcachedata.Length) {
					process.Resume();
					throw new RteProviderException(this, "Error: Loaded tag size did not match cache tag size. Is this tag overwritten in a modpak? Is your modpak built on the most up to date cache?");
				}
				if (RuntimeTagDataMap.Length == 0) {
					RuntimeTagDataMap = new byte[CurrentRuntimeTagData.Length];
					for (int i = 0; i < CurrentRuntimeTagData.Length; i++) {
						if (CurrentRuntimeTagData[i] == tagcachedata[i]) {
							RuntimeTagDataMap[i] = 1;
						}
					}
				}
				if (tagcachedata.Length != RuntimeTagDataMap.Length) {
					process.Resume();
					throw new RteProviderException(this, "Error: Loaded tag has changed size since initial poke! Try closing and reopening the tag.");
				}
				if (isModPackage) {
					List<int> modpaktagindices = new List<int>();
					for (int j = 0; j < modpak.TagCache.Count; j++) {
						if (modpak.TagCache.TryGetCachedTag(j, out var taginstance) && !( (CachedTagHaloOnline)taginstance ).IsEmpty()) {
							modpaktagindices.Add(j);
						}
					}
					foreach (uint tagreffixup in TagReferenceFixups) {
						int editortagref = BitConverter.ToInt32(editordata, (int)(tagreffixup - headersize));
						if (!modpak.BaseCacheReference.TagCache.TryGetCachedTag(editortagref, out var _)) {
							int paktagcount = modpaktagindices.Count((int x) => x < editortagref);
							editortagref = 65534 - paktagcount;
							byte[] newvalue = BitConverter.GetBytes(editortagref);
							newvalue.CopyTo(CurrentRuntimeTagData, tagreffixup - headersize);
						}
					}
				}
				int patchedbytes = 0;
				for (int k = 0; k < tagcachedata.Length; k++) {
					if (RuntimeTagDataMap[k] == 1) {
						CurrentRuntimeTagData[k] = editordata[k];
						patchedbytes++;
					}
				}
				processStream.Seek(address + headersize, SeekOrigin.Begin);
				processStream.Write(CurrentRuntimeTagData, 0, CurrentRuntimeTagData.Length);
				processStream.Flush();
				process.Resume();
				stopWatch.Stop();
				Console.WriteLine($"Patched {patchedbytes} bytes in {(float)stopWatch.ElapsedMilliseconds / 1000f} seconds");
			}
		}

		private static int ResolveTagIndex(int tagindex, bool isModPackage, GameCacheModPackage modpak) {
			if (isModPackage && !modpak.BaseCacheReference.TagCache.TryGetCachedTag(tagindex, out var _)) {
				int paktagcount = 0;
				for (int i = 0; i < tagindex; i++) {
					if (modpak.TagCache.TryGetCachedTag(i, out var taginstance) && !( (CachedTagHaloOnline)taginstance ).IsEmpty()) {
						paktagcount++;
					}
				}
				tagindex = 65534 - paktagcount;
			}
			return tagindex;
		}

		private void ReadHeaderValues(EndianReader reader, ref uint currenttotalsize, ref uint headersize, ref List<uint> TagReferenceFixups) {
			currenttotalsize = reader.ReadUInt32();
			short numDependencies = reader.ReadInt16();
			short numDataFixups = reader.ReadInt16();
			short numResourceFixups = reader.ReadInt16();
			short numTagReferenceFixups = reader.ReadInt16();
			reader.BaseStream.Position += 20L;
			reader.BaseStream.Position += 4 * numDependencies;
			reader.BaseStream.Position += 4 * numDataFixups;
			reader.BaseStream.Position += 4 * numResourceFixups;
			for (int i = 0; i < numTagReferenceFixups; i++) {
				TagReferenceFixups.Add(reader.ReadUInt32() - 1073741824);
			}
			headersize = CalculateMemoryHeaderSize(36, numDependencies, numDataFixups, numResourceFixups, numTagReferenceFixups);
		}

		public uint CalculateMemoryHeaderSize(int TagHeaderSize, int numDependencies, int numDataFixups, int numResourceFixups, int numTagReferenceFixups) {
			uint size = (uint)(TagHeaderSize + numDependencies * 4 + numDataFixups * 4 + numResourceFixups * 4 + numTagReferenceFixups * 4);
			return (uint)( ( size + 15 ) & -16 );
		}

		private static uint GetTagAddress(ProcessMemoryStream stream, int tagIndex) {
			BinaryReader reader = new BinaryReader(stream);
			reader.BaseStream.Position = 36352008L;
			int maxIndex = 65535;
			if (tagIndex >= maxIndex) {
				return 0u;
			}
			reader.BaseStream.Position = 36351996L;
			uint tagIndexTableAddress = reader.ReadUInt32();
			if (tagIndexTableAddress == 0) {
				return 0u;
			}
			reader.BaseStream.Position = tagIndexTableAddress + tagIndex * 4;
			int addressIndex = reader.ReadInt32();
			if (addressIndex < 0) {
				return 0u;
			}
			reader.BaseStream.Position = 36351992L;
			uint tagAddressTableAddress = reader.ReadUInt32();
			if (tagAddressTableAddress == 0) {
				return 0u;
			}
			reader.BaseStream.Position = tagAddressTableAddress + addressIndex * 4;
			return reader.ReadUInt32();
		}

		public void DumpData(string filename, byte[] data) {
			using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
				fs.Write(data, 0, data.Length);
			}
		}

		public ProcessMemoryStream CreateStream(IRteTarget target) {
			Process process = Process.GetProcessById((int)target.Id);
			if (process == null) {
				throw new RteTargetNotAvailableException(this, "Target process could not be found.");
			}
			return new ProcessMemoryStream(process);
		}

		public long GetTagMemoryAddress(ProcessMemoryStream stream, GameCache cache, CachedTag instance) {
			GameCacheModPackage modpak = null;
			bool isModPackage = false;
			if (cache is GameCacheModPackage) {
				modpak = (GameCacheModPackage)cache;
				isModPackage = true;
			}
			return GetTagAddress(stream, ResolveTagIndex(instance.Index, isModPackage, modpak));
		}
	}
}
