using EpsilonLib.Core;
using Stylet;
using System;
using System.Linq;

namespace Epsilon
{
    class ViewManager : Stylet.ViewManager
    {
        public ViewManager(ViewManagerConfig config) : base(config)
        {
        }

        protected override Type LocateViewForModel(Type modelType)
        {
            var explicitAttribute = modelType
                .GetCustomAttributes(typeof(ExplicitViewAttribute), false)
                .Cast<ExplicitViewAttribute>().FirstOrDefault();

            if(explicitAttribute != null)
            {
                return explicitAttribute.ViewType;
            }

            return base.LocateViewForModel(modelType);
        }
    }
}
