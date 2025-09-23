using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ConfigurationDataAccess(string connectionString) : DataAccess<Configuration>(connectionString, Configuration.Metadata)
{
}
