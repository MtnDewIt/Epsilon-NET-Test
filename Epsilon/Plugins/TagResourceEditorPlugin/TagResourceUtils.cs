using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache.Gen3;
using TagTool.Cache.Resources;
using TagTool.Tags;
using TagTool.Tags.Resources;

namespace TagResourceEditorPlugin
{
    class TagResourceUtils
    {
        // TODO: move some of this back out to tagtool?

        public static object GetResourceDefinition(ResourceCache cache, TagResourceReference resourceReference)
        {
            if (cache is ResourceCacheGen3 gen3Cache)
                gen3Cache.LoadResourceCache();

            var definitionType = GetTagResourceDefinitionType(cache, resourceReference);

            return cache.GetResourceDefinition(resourceReference, definitionType);
        }

        public static Type GetTagResourceDefinitionType(ResourceCache cache, TagResourceReference resourceReference)
        {
            return GetTagResourceDefinitionType(GetTagResourceFromReference(cache, resourceReference).ResourceType);
        }

        public static Type GetTagResourceDefinitionType(ResourceData resource)
        {
            return GetTagResourceDefinitionType(resource.ResourceType);
        }

        public static ResourceData GetTagResourceFromReference(ResourceCache cache, TagResourceReference resourceReference)
        {
            ResourceData resource = null;
            if (resourceReference.HaloOnlinePageableResource != null)
                resource = resourceReference.HaloOnlinePageableResource.Resource;
            else if (cache is ResourceCacheGen3 gen3Cache)
                resource = gen3Cache.GetTagResourceFromReference(resourceReference);
            return resource;
        }

        private static Type GetTagResourceDefinitionType(TagResourceTypeGen3 type)
        {
            switch (type)
            {
                case TagResourceTypeGen3.Animation:
                    return typeof(ModelAnimationTagResource);

                case TagResourceTypeGen3.Bink:
                    return typeof(BinkResource);

                case TagResourceTypeGen3.Bitmap:
                    return typeof(BitmapTextureInteropResource);

                case TagResourceTypeGen3.BitmapInterleaved:
                    return typeof(BitmapTextureInterleavedInteropResource);

                case TagResourceTypeGen3.Collision:
                    return typeof(StructureBspTagResources);

                case TagResourceTypeGen3.Pathfinding:
                    return typeof(StructureBspCacheFileTagResources);

                case TagResourceTypeGen3.RenderGeometry:
                    return typeof(RenderGeometryApiResourceDefinition);

                case TagResourceTypeGen3.Sound:
                    return typeof(SoundResourceDefinition);

                default:
                    throw new TypeLoadException(type.ToString());
            }
        }
    }
}
