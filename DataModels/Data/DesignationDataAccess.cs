using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class DesignationDataAccess : DataAccess<Designation>
{
    public DesignationDataAccess(string connectionString) : base(connectionString, Designation.Metadata)
    {
    }
}
