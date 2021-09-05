using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.Framework.Model.Enums
{
    public enum FileResponseType
    {
        Blob = 1 << 0,
        Binary = 1 << 1,
    }
}
