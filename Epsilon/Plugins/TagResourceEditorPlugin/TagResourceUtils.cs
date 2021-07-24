using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagTool.Cache;
using TagTool.Cache.Gen3;
using TagTool.Cache.HaloOnline;
using TagTool.Cache.Resources;
using TagTool.Tags;
using TagTool.Tags.Resources;

namespace TagResourceEditorPlugin
{
    class TagResourceUtils
    {
        // TODO: move some of this back out to tagtool?

        public static object GetResourceDefinition(ResourceCache cache, Type definitionType, TagResourceReference resourceReference)
        {
            return cache.GetResourceDefinition(resourceReference, definitionType);
        }

        public static Type GetTagResourceDefinitionType(GameCache cache, TagResourceReference resourceReference)
        {
            if(cache is GameCacheGen3 gen3Cache)
            {
                var resource = gen3Cache.ResourceCacheGen3.GetTagResourceFromReference(resourceReference);
                var gestalt = gen3Cache.ResourceCacheGen3.ResourceGestalt;
                if (resource.ResourceTypeIndex < 0 || resource.ResourceTypeIndex >= gestalt.ResourceDefinitions.Count)
                    return null;

                var typeDef = gestalt.ResourceDefinitions[resource.ResourceTypeIndex];
                var typeName = cache.StringTable.GetString(typeDef.Name);
                return GetTagResourceDefinitionTypeGen3(typeName);
            }
            else if(cache is GameCacheHaloOnlineBase)
            {
                if (resourceReference.HaloOnlinePageableResource == null)
                    return null;

                return resourceReference.HaloOnlinePageableResource.GetDefinitionType();
            }

            return null;
        }

        private static Type GetTagResourceDefinitionTypeGen3(string name)
        {
            switch(name)
            {
                case "model_animation_tag_resource":
                    return typeof(ModelAnimationTagResource);
                case "bink_resource":
                    return typeof(ModelAnimationTagResource);
                case "bitmap_texture_interop_resource":
                    return typeof(BitmapTextureInteropResource);
                case "bitmap_texture_interleaved_interop_resource":
                    return typeof(BitmapTextureInterleavedInteropResource);
                case "structure_bsp_tag_resources":
                    return typeof(StructureBspTagResources);
                case "structure_bsp_cache_file_tag_resources":
                    return typeof(StructureBspCacheFileTagResources);
                case "sound_resource_definition":
                    return typeof(SoundResourceDefinition);
                case "render_geometry_api_resource_definition":
                    return typeof(RenderGeometryApiResourceDefinition);
                default:
                    return null;
            }
        }

        private static Type GetTagResourceDefinitionTypeHaloOnline(TagResourceTypeGen3 type)
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
