using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EpsilonLib.Shell.TreeModels
{
    public static class VisualTreeEx
    {
        public static IEnumerable<T> FindAncestors<T>(this DependencyObject node) where T : DependencyObject
        {
            while(node != null)
            {
                if(node is T nodeAsT)
                    yield return nodeAsT;

                node = VisualTreeHelper.GetParent(node);
            }
        }
    }
}
