using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class SupplierDataAccess(string connectionString) : DataAccess<Supplier>(connectionString, Supplier.Metadata)
{
}
