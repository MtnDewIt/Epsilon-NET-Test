using HaloShaderGenerator.Generator;
using HaloShaderGenerator.Particle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TagTool.Tags.Definitions.RenderMethod;

namespace RenderMethodEditorPlugin.ShaderMethods.Particle
{
    class ParticleMethod : MethodParser
    {
        public override string GetMethodName(int methodIndex)
        {
            return ((ParticleMethods)methodIndex).ToString();
        }

        public override string GetOptionName(int methodIndex, int optionIndex)
        {
            switch ((ParticleMethods)methodIndex)
            {
                case ParticleMethods.Albedo:
                    return ((Albedo)optionIndex).ToString();
                case ParticleMethods.Blend_Mode:
                    return ((Blend_Mode)optionIndex).ToString();
                case ParticleMethods.Specialized_Rendering:
                    return ((Specialized_Rendering)optionIndex).ToString();
                case ParticleMethods.Lighting:
                    return ((Lighting)optionIndex).ToString();
                case ParticleMethods.Render_Targets:
                    return ((Render_Targets)optionIndex).ToString();
                case ParticleMethods.Depth_Fade:
                    return ((Depth_Fade)optionIndex).ToString();
                case ParticleMethods.Black_Point:
                    return ((Black_Point)optionIndex).ToString();
                case ParticleMethods.Fog:
                    return ((Fog)optionIndex).ToString();
                case ParticleMethods.Frame_Blend:
                    return ((Frame_Blend)optionIndex).ToString();
                case ParticleMethods.Self_Illumination:
                    return ((Self_Illumination)optionIndex).ToString();
            }
            return "";
        }

        public override string GetOptionDescription(int methodIndex, int optionIndex)
        {
            switch ((ParticleMethods)methodIndex)
            {
                case ParticleMethods.Albedo:
                    switch ((Albedo)optionIndex)
                    {
                        case Albedo.Diffuse_Only:
                            return "Albedo from base map only.";
                    }
                    break;

                case ParticleMethods.Lighting:
                    switch ((Lighting)optionIndex)
                    {
                        case Lighting.None:
                            return "No lighting applied.";
                        case Lighting.Per_Pixel_Ravi_Order_3:
                            return "Use per pixel lighting";
                        case Lighting.Per_Vertex_Ravi_Order_0:
                            return "Use per vertex lighting";
                    }
                    break;

                case ParticleMethods.Frame_Blend:
                    switch ((Frame_Blend)optionIndex)
                    {
                        case Frame_Blend.Off:
                            return "No frame blending";
                        case Frame_Blend.On:
                            return "Blend 2 frames using different texture sampling coordinates";
                    }
                    break;

            }
            return "No description available.";
        }

        public override IShaderGenerator BuildShaderGenerator(List<RenderMethodDefinitionOptionIndex> shaderOptions)
        {
            var albedo = (Albedo)shaderOptions[0].OptionIndex;
            var blend_mode = (Blend_Mode)shaderOptions[1].OptionIndex;
            var specialized_rendering = (Specialized_Rendering)shaderOptions[2].OptionIndex;
            var lighting = (Lighting)shaderOptions[3].OptionIndex;
            var render_target = (Render_Targets)shaderOptions[4].OptionIndex;
            var depth_fade = (Depth_Fade)shaderOptions[5].OptionIndex;
            var black_point = (Black_Point)shaderOptions[6].OptionIndex;
            var fog = (Fog)shaderOptions[7].OptionIndex;
            var frame_blend = (Frame_Blend)shaderOptions[8].OptionIndex;
            var self_illumination = (Self_Illumination)shaderOptions[9].OptionIndex;
           
             
            return new ParticleGenerator(albedo, blend_mode, specialized_rendering, lighting, render_target, depth_fade, black_point, fog, frame_blend, self_illumination);
        }
    }
}
