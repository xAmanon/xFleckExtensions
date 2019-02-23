using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fleck.Extensions
{
    public class Utils
    {
        public static long GetTimestemp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        /// <summary>
        /// 判断指定的类型 <paramref name="type"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
        /// </summary>
        /// <param name="type">需要测试的类型。</param>
        /// <param name="generic">泛型接口类型，传入 typeof(IXxx<>)</param>
        /// <returns>如果是泛型接口的子类型，则返回 true，否则返回 false。</returns>
        public static Type GetImplementedRawGeneric(Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (generic == null) throw new ArgumentNullException(nameof(generic));

            // 测试接口。
            var theRawGenericType = type.GetInterfaces().FirstOrDefault(IsTheRawGenericType);
            if (theRawGenericType != null) return theRawGenericType;

            // 测试类型。
            while (type != null && type != typeof(object))
            {
                var isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return type;
                type = type.BaseType;
            }

            // 没有找到任何匹配的接口或类型。
            return null;

            // 测试某个类型是否是指定的原始接口。
            bool IsTheRawGenericType(Type test) => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }
    }
}
