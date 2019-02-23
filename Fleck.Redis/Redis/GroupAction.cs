using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions.Redis
{
    public enum GroupAction : byte
    {
        // These numbers are used by the protocol, do not change them and always use explicit assignment
        // when adding new items to this enum. 0 is intentionally omitted
        Add = 1,
        Remove = 2,
    }
}
