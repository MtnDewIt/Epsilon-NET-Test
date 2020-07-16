using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
