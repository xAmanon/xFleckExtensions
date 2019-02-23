using System;
using System.Collections.Generic;
using System.Text;

namespace Fleck.Extensions
{
    [Flags]
    public enum TransferFormat
    {
        Binary = 0x01,
        Text = 0x02
    }
}
