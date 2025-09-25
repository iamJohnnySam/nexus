using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ProductModuleDataAccess(string connectionString) : DataAccess<ProductModule>(connectionString, ProductModule.Metadata)
{

}
