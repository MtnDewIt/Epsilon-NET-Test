using Epsilon;
using Epsilon.RTE;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using TagTool.Cache;

namespace Epsilon
{
	[Export(typeof(ITagEditorPluginProvider))]
	public class EpsilonPluginProvider : ITagEditorPluginProvider
	{

		private readonly IRteService _rteService;

		public string DisplayName => "Definition";
		public int SortOrder => -1;

		[ImportingConstructor]
		public EpsilonPluginProvider(IRteService rteService) {
			_rteService = rteService;
		}

		public async Task<ITagEditorPlugin> CreateAsync(TagEditorContext context) {
			return new DefinitionEditorViewModel(context, _rteService);
		}

		public bool ValidForTag(ICacheFile cache, CachedTag tag) {
			return true;
		}

	}
}
