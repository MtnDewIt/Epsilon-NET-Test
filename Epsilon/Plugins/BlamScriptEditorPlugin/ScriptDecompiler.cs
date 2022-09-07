﻿using System;
using System.IO;
using TagTool.Cache;
using TagTool.Common;
using TagTool.IO;
using TagTool.Scripting;
using TagTool.Tags.Definitions;
using System.CodeDom.Compiler;
using System.Text;

namespace BlamScriptEditorPlugin
{
    class ScriptDecompiler
    {
        private GameCache Cache;
        private Scenario Definition;

        public ScriptDecompiler(GameCache cache, Scenario definition)
        {
            Cache = cache;
            Definition = definition;
        }

    public string Decompile()
    {
        var scriptFileStream = new MemoryStream();

        using (var scriptWriter = new StreamWriter(scriptFileStream))
        using (var scriptStringStream = new MemoryStream(Definition.ScriptStrings))
        using (var scriptStringReader = new BinaryReader(scriptStringStream))
        using (var baseTextWriter = new System.IO.StringWriter())
        using (var indentWriter = new IndentedTextWriter(scriptWriter, "	"))
        {
            indentWriter.Indent = 0;

            //
            // Export scenario script globals
            //

            foreach (var scriptGlobal in Definition.Globals)
            {
                indentWriter.Write($"(global {GetHsTypeAsString(Cache.Version, scriptGlobal.Type).ToSnakeCase()} {scriptGlobal.Name}");

                var expr = Definition.ScriptExpressions[scriptGlobal.InitializationExpressionHandle.Index];

                WriteExpression(expr, expr, scriptStringReader, indentWriter, false);

                indentWriter.WriteLine(')');
            }

            indentWriter.WriteLine();

            //
            // Export scenario scripts
            //

            foreach (var script in Definition.Scripts)
            {
                indentWriter.Write($"(script {script.Type.ToString().ToSnakeCase()} {GetHsTypeAsString(Cache.Version, script.ReturnType).ToSnakeCase()} ");

                if (script.Parameters.Count == 0)
                {
                    indentWriter.WriteLine(script.ScriptName);
                }
                else
                {
                    indentWriter.Write($"({script.ScriptName}");

                    foreach (var parameter in script.Parameters)
                    {
                        indentWriter.Write($" ({GetHsTypeAsString(Cache.Version, parameter.Type).ToSnakeCase()} {parameter.Name})");
                    }

                    indentWriter.WriteLine(')');
                }
                var expr = Definition.ScriptExpressions[script.RootExpressionHandle.Index];
                var shouldSkip = false;
                if (expr.Opcode == 0)
                    shouldSkip = true;
                WriteExpression(expr, expr, scriptStringReader, indentWriter, shouldSkip);

                indentWriter.WriteLine(')');

                indentWriter.WriteLine();
            }
        }
        return Encoding.UTF8.GetString(scriptFileStream.ToArray());
    }

    private string ReadScriptString(BinaryReader reader, long address)
        {
            var result = "";

            reader.BaseStream.Position = address;
            for (char c; (c = reader.ReadChar()) != 0x00; result += c) ;

            return result;
        }

        private string OpcodeLookup(ushort Opcode)
        {
            string result = "unk_op";

            if (ScriptInfo.Scripts[(Cache.Version, Cache.Platform)].ContainsKey(Opcode))
                result = ScriptInfo.Scripts[(Cache.Version, Cache.Platform)][Opcode].Name;

            return result;
        }

        private void WriteValueExpression(HsSyntaxNode parentExprk, HsSyntaxNode expr, BinaryReader stringReader, IndentedTextWriter indentWriter)
        {
            var exprIndex = (ushort)(Definition.ScriptExpressions.IndexOf(expr));
            var nextExpr = Definition.ScriptExpressions[exprIndex].NextExpressionHandle.Index;
            var nextExprValid = false;
            if (nextExpr < Definition.ScriptExpressions.Count)
                nextExprValid = GetHsTypeAsString(Cache.Version, Definition.ScriptExpressions[nextExpr].ValueType) != "Invalid";

            var valueType = GetHsTypeAsString(Cache.Version, expr.ValueType);

            if (valueType != "FunctionName")
                indentWriter.Write(" ");

            switch (valueType)
            {
                case "FunctionName":
                    indentWriter.Write(expr.StringAddress == 0 ? OpcodeLookup(expr.Opcode) : ReadScriptString(stringReader, expr.StringAddress));
                    if ((expr.Opcode == 0 || expr.Opcode == 1 || expr.Opcode == 22) && nextExprValid)
                        indentWriter.WriteLine();
                    else if (expr.Opcode < 19 && nextExprValid)
                        indentWriter.Write(' ');
                    break; //Trust the string table, its faster than going through the dictionary with OpcodeLookup.

                case "Boolean":
                    indentWriter.Write(expr.Data[0] == 0 ? "false" : "true");
                    break;

                case "Real":
                    indentWriter.Write(BitConverter.ToSingle(SortExpressionDataArray(Cache.Endianness, expr.Data, 4), 0));
                    break;

                case "Short":
                    indentWriter.Write(BitConverter.ToInt16(SortExpressionDataArray(Cache.Endianness, expr.Data, 2), 0));
                    break;

                case "Long":
                    indentWriter.Write(BitConverter.ToInt32(SortExpressionDataArray(Cache.Endianness, expr.Data, 4), 0));
                    break;

                case "String":
                    indentWriter.Write(expr.StringAddress == 0 ? "none" : $"\"{ReadScriptString(stringReader, expr.StringAddress)}\"");
                    break;

                case "Script":
                    indentWriter.Write(Definition.Scripts[BitConverter.ToInt16(SortExpressionDataArray(Cache.Endianness, expr.Data, 2), 0)].ScriptName);
                    break;

                case "StringId":
                    indentWriter.Write($"\"{Cache.StringTable.GetString(new StringId(BitConverter.ToUInt32(SortExpressionDataArray(Cache.Endianness, expr.Data, 4), 0)))}\"");
                    break;

                case "GameDifficulty":
                    switch (BitConverter.ToInt16(SortExpressionDataArray(Cache.Endianness, expr.Data, 2), 0))
                    {
                        case 0: indentWriter.Write("easy"); break;
                        case 1: indentWriter.Write("normal"); break;
                        case 2: indentWriter.Write("heroic"); break;
                        case 3: indentWriter.Write("legendary"); break;
                        default: throw new NotImplementedException();
                    }
                    break;

                case "Folder":
                case "Unit":
                case "AnimationGraph":
                case "Object":
                case "Device":
                case "CutsceneCameraPoint":
                case "CutsceneFlag":
                case "TriggerVolume":
                case "UnitSeatMapping":
                case "Vehicle":
                case "VehicleName":
                case "Effect":
                case "Sound":
                case "LoopingSound":
                case "AnyTag":
                case "ObjectName":
                case "Scenery":
                case "Ai":
                case "PointReference":
                case "ObjectDefinition":
                case "CutsceneTitle":
                case "ZoneSet":
                case "Damage":
                case "StartingProfile":
                case "DeviceGroup":
                    indentWriter.Write(expr.StringAddress == 0 ? "none" : $"\"{ReadScriptString(stringReader, expr.StringAddress)}\"");
                    break;

                case "Team":
                case "AiCommandScript":
                case "AiLine":
                    indentWriter.Write(expr.StringAddress == 0 ? "none" : $"{ReadScriptString(stringReader, expr.StringAddress)}");
                    break;

                default:
                    indentWriter.Write($"<UNIMPLEMENTED VALUE: {expr.Flags.ToString()} {valueType}>");
                    break;
            }

        }

        private void WriteGroupExpression(HsSyntaxNode parentExpr, HsSyntaxNode expr, BinaryReader stringReader, IndentedTextWriter indentWriter, bool shouldSkip)
        {
            bool shouldSkipFirst = shouldSkip;

            if (!shouldSkip)
            {
                if (parentExpr.Opcode > 19 && parentExpr.Opcode != 22)
                    indentWriter.Write(' ');

                indentWriter.Indent++;
                indentWriter.Write('(');
            }

            var prevExpr = expr;
            for (var exprIndex = (ushort)(Definition.ScriptExpressions.IndexOf(expr) + 1); GetHsTypeAsString(Cache.Version, Definition.ScriptExpressions[exprIndex].ValueType) != "Invalid"; exprIndex = Definition.ScriptExpressions[exprIndex].NextExpressionHandle.Index)
            {
                if (shouldSkipFirst)
                {
                    shouldSkipFirst = false;
                    continue;
                }
                var nextExpr = Definition.ScriptExpressions[exprIndex];
                if (prevExpr.Flags == HsSyntaxNodeFlags.GlobalsReference && (nextExpr.Flags == HsSyntaxNodeFlags.Group || nextExpr.Flags == HsSyntaxNodeFlags.ScriptReference))
                    indentWriter.Write(' ');
                WriteExpression(expr, nextExpr, stringReader, indentWriter, false);
                prevExpr = nextExpr;

                if (Definition.ScriptExpressions[exprIndex].NextExpressionHandle.Index == ushort.MaxValue || Definition.ScriptExpressions[exprIndex].NextExpressionHandle.Index + 1 > Definition.ScriptExpressions.Count)
                    break;
            }

            if (!shouldSkip)
            {
                if (parentExpr.Opcode < 3 || parentExpr.Opcode == 22)
                    indentWriter.WriteLine(')');
                else
                    indentWriter.Write(')');

                indentWriter.Indent--;
            }
        }

        private void WriteExpression(HsSyntaxNode parentExpr, HsSyntaxNode expr, BinaryReader stringReader, IndentedTextWriter indentWriter, bool shouldSkip)
        {
            switch (expr.Flags)
            {
                case HsSyntaxNodeFlags.ScriptReference:
                case HsSyntaxNodeFlags.Group:
                    WriteGroupExpression(parentExpr, expr, stringReader, indentWriter, shouldSkip);
                    break;

                case HsSyntaxNodeFlags.Expression:
                    WriteValueExpression(parentExpr, expr, stringReader, indentWriter);
                    break;

                case HsSyntaxNodeFlags.GlobalsReference:
                case HsSyntaxNodeFlags.ParameterReference:
                    if (parentExpr.Opcode > 19)
                        indentWriter.Write(' ');
                    indentWriter.Write(expr.StringAddress == 0 ? "none" : ReadScriptString(stringReader, expr.StringAddress));
                    break;

                default:
                    indentWriter.Write($"<UNIMPLEMENTED EXPR: {expr.Flags.ToString()} {GetHsTypeAsString(Cache.Version, expr.ValueType)}>");
                    break;
            }
        }

        private string GetHsTypeAsString(CacheVersion version, HsType type)
        {
            switch (version)
            {
                case CacheVersion.Halo3Retail:
                    return type.Halo3Retail.ToString();

                case CacheVersion.Halo3ODST:
                    return type.Halo3ODST.ToString();

                case CacheVersion.HaloOnlineED:
                case CacheVersion.HaloOnline106708:
                    return type.HaloOnline.ToString();

                default:
                    Console.WriteLine($"WARNING: No HsType found for cache \"{version}\". Defaulting to HaloOnline");
                    return type.HaloOnline.ToString();
            }
        }

        private byte[] SortExpressionDataArray(EndianFormat format, byte[] data, int dataLength)
        {
            if (format == EndianFormat.BigEndian)
            {
                byte[] newData = new byte[dataLength];

                // reverse the data array, but only to the specified length
                for (int i = 0; i < dataLength; i++)
                    newData[i] = data[(dataLength - 1) - i];

                return newData;
            }

            return data;
        }
    }
}