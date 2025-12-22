using System.Reflection;
using System.Runtime.CompilerServices;

namespace HaselCommon.Extensions;

public static class FieldInfoExtensions
{
    extension(FieldInfo info)
    {
        public bool IsFixed()
        {
            return info.GetCustomAttributes(typeof(FixedBufferAttribute), false).Cast<FixedBufferAttribute>().FirstOrDefault() != null;
        }

        public Type GetFixedType()
        {
            return info.GetCustomAttributes(typeof(FixedBufferAttribute), false).Cast<FixedBufferAttribute>().Single().ElementType;
        }

        public int GetFixedSize()
        {
            return info.GetCustomAttributes(typeof(FixedBufferAttribute), false).Cast<FixedBufferAttribute>().Single().Length;
        }

        public int GetFieldOffset()
        {
            var attrs = info.GetCustomAttributes(typeof(FieldOffsetAttribute), false);

            return attrs.Length != 0
                ? attrs.Cast<FieldOffsetAttribute>().Single().Value
                : info.GetFieldOffsetSequential();
        }

        public int GetFieldOffsetSequential()
        {
            if (info.DeclaringType == null)
                throw new Exception($"Unable to access declaring type of field {info.Name}");

            var fields = info.DeclaringType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var offset = 0;

            foreach (var field in fields)
            {
                if (field == info)
                    return offset;

                offset += field.FieldType.SizeOf();
            }

            throw new Exception("Field not found");
        }
    }
}
