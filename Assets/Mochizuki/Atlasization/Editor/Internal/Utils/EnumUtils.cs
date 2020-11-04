/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Linq;
using System.Runtime.Serialization;

namespace Mochizuki.Atlasization.Internal.Utilities
{
    internal static class EnumUtils
    {
        public static string GetEnumMemberValue<T>(string value) where T : System.Enum
        {
            var member = typeof(T).GetMember(value);
            var attr = member[0].GetCustomAttributes(false).OfType<EnumMemberAttribute>().First();
            return attr?.Value;
        }
    }
}