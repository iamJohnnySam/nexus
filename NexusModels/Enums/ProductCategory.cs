using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusModels.Enums
{
    public enum ProductCategory
    {
        None,
        [Description("Load Port")]
        LoadPort,
        [Description("EFEM")]
        EFEM,
        [Description("End Effector")]
        EndEffector,
        [Description("Load Lock")]
        LoadLock,
        Aligner,
        Robot
    }
}
