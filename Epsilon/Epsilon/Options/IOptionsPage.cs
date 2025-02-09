using Stylet;

namespace Epsilon.Options
{
    public interface IOptionsPage : IScreen
    {
        string Category { get; }

        bool IsDirty { get; set; }

        void Apply();

		void Save();
	}

    public class ProvideOptionsPageAttribute
    {
        public string Category { get; set; }
    }
}
