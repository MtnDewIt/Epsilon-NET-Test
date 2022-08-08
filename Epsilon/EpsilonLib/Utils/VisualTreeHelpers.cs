using System.Collections.Generic;
using System.Windows.Media;

namespace EpsilonLib.Utils
{
    public static class VisualTreeHelpers
    {
        public static IEnumerable<T> FindAncestors<T>(this Visual visual)
        {
            while (visual != null)
            {
                if (visual is T value)
                    yield return value;

                visual = VisualTreeHelper.GetParent(visual) as Visual;
            }
        }
    }
}
