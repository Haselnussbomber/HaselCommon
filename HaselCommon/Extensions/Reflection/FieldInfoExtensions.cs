using System.Reflection;
using System.Runtime.CompilerServices;

namespace HaselCommon.Extensions;

public static class FieldInfoExtensions
{
    extension(FieldInfo info)
    {
        public bool IsFixed => Attribute.IsDefined(info, typeof(FixedBufferAttribute), false);

        public Type FixedType => info.GetCustomAttribute<FixedBufferAttribute>()!.ElementType;

        public int FixedSize => info.GetCustomAttribute<FixedBufferAttribute>()!.Length;

        public int FieldOffset
        {
            get
            {
                if (Attribute.IsDefined(info, typeof(FieldOffsetAttribute), false))
                    return info.GetCustomAttribute<FieldOffsetAttribute>()!.Value;

                return info.GetFieldOffsetSequential();
            }
        }

        private int GetFieldOffsetSequential()
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
