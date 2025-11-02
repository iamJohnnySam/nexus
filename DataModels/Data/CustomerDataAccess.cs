using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class CustomerDataAccess(string connectionString) : DataAccess<Customer>(connectionString, Customer.Metadata)
{
    public Dictionary<int, Customer> CustomersCache { get; set; }   = [];

    public override async Task<Customer?> GetByIdAsync(int id)
    {
        if (!CustomersCache.ContainsKey(id))
        {
            CustomersCache[id] = (await base.GetByIdAsync(id))!;
        }
        return CustomersCache[id];
    }

    public override async Task UpdateAsync (Customer entity)
    {
        await base.UpdateAsync(entity);
        CustomersCache[entity.CustomerId] = entity;
    }

    public override async Task DeleteAsync (Customer entity)
    {
        await base.DeleteAsync(entity);
        CustomersCache.Remove(entity.CustomerId);
    }
}
