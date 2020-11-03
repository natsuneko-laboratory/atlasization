/*-------------------------------------------------------------------------------------------
 * Copyright (c) Natsuneko. All rights reserved.
 * Licensed under the MIT License. See LICENSE in the project root for license information.
 *------------------------------------------------------------------------------------------*/

using System.Runtime.Serialization;

namespace Mochizuki.Atlasization.Internal.Enum
{
    internal enum AtlasSize
    {
        [EnumMember(Value = "1K")]
        One,

        [EnumMember(Value = "2K")]
        Two,

        [EnumMember(Value = "4K")]
        Four,

        [EnumMember(Value = "8K")]
        Eight
    }
}