using System;
using System.Collections;
using System.Reflection;
using TagTool.Cache;

namespace TagStructEditor.Common
{
    public static class ObjectExtensions
    {
        public static object DeepCloneV2(this object data)
        {
            if (data == null)
                return null;

            var type = data.GetType();
            if (type.IsValueType)
                return data;

            switch (data)
            {
                case CachedTag tag:
                    return tag;
                case IList list:
                    {
                        if (type.IsArray)
                        {
                            var cloned = (IList)Activator.CreateInstance(type, new object[] { list.Count });
                            for (int i = 0; i < cloned.Count; i++)
                                cloned[i] = list[i].DeepCloneV2();
                            return cloned;
                        }
                        else
                        {
                            var cloned = (IList)Activator.CreateInstance(type);
                            foreach (var element in list)
                                cloned.Add(element.DeepCloneV2());
                            return cloned;
                        }
                    }
                case ICloneable cloneable:
                    return cloneable.Clone();
                default:
                    {
                        var cloned = Activator.CreateInstance(type);
                        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                            field.SetValue(cloned, field.GetValue(data).DeepCloneV2());
                        return cloned;
                    }
            }
        }

        public static T DeepCloneV2<T>(this T data)
        {
            return (T)DeepCloneV2((object)data);
        }
    }
}
