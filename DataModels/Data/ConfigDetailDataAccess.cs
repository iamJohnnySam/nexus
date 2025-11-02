using DataModels.DataTools;
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ConfigDetailDataAccess(string connectionString, ConfigurationDataAccess configurationDB, SpecificationDataAccess specificationDB) : DataAccess<ConfigDetail>(connectionString, ConfigDetail.Metadata)
{
    ConfigurationDataAccess ConfigurationDB = configurationDB;
    SpecificationDataAccess SpecificationDB = specificationDB;
    private async Task GetItems(ConfigDetail item)
    {
        item.Configuration = await ConfigurationDB.GetByIdAsync(item.ConfigurationId);
        item.Specification = await SpecificationDB.GetByIdAsync(item.SpecificationId);
    }

    internal override async Task GetAllAsync()
    {
        await base.GetAllAsync();
        foreach (ConfigDetail item in AllItems)
        {
            await GetItems(item);
        }
    }

    public override async Task<ConfigDetail?> GetByIdAsync(int id)
    {
        var item = await base.GetByIdAsync(id);
        if (item != null)
            await GetItems(item);
        return item;
    }


    public async Task<List<ConfigDetail>> GetAllConfigDetailsLatestRev()
    {
        var sql = @"
        SELECT cd.*
        FROM ConfigDetail cd
        INNER JOIN (
            SELECT ConfigurationId, SpecificationId, MAX(Revision) AS MaxRev
            FROM ConfigDetail
            GROUP BY ConfigurationId, SpecificationId
        ) latest
        ON cd.ConfigurationId = latest.ConfigurationId
        AND cd.SpecificationId = latest.SpecificationId
        AND cd.Revision = latest.MaxRev;";

        var result = await QueryAsync(sql);

        foreach (ConfigDetail configDetail in result)
        {
            await GetItems(configDetail);
        }
        return result.ToList();
    }

    public async Task<ConfigDetail?> GetBySpecificationId(int configurationId, int specificationId)
    {
        var sql = "SELECT * FROM ConfigDetail WHERE SpecificationId = @specificationId AND ConfigurationId = @configurationId";
        ConfigDetail? result = await QueryFirstOrDefaultAsync(sql, new { specificationId, configurationId });
        if (result != null)
            await GetItems(result);
        return result;
    }
    public async Task<ConfigDetail?> GetBySpecificationIdLatestRevAsync(int configurationId, int specificationId)
    {
        var sql = @"
        SELECT *
        FROM ConfigDetail
        WHERE ConfigurationId = @configurationId
          AND SpecificationId = @specificationId
        ORDER BY Revision DESC
        LIMIT 1;";

        ConfigDetail? result = await QueryFirstOrDefaultAsync(sql, new { specificationId, configurationId });

        if (result != null)
            await GetItems(result);            
        return result;
    }
    public async Task<List<ConfigDetail>> GetAllRevisionsAsync(int configurationId, int specificationId)
    {
        var sql = @"
        SELECT *
        FROM ConfigDetail
        WHERE ConfigurationId = @configurationId
          AND SpecificationId = @specificationId
        ORDER BY Revision DESC;";

        List<ConfigDetail> result = await QueryAsync(sql, new { specificationId, configurationId });

        foreach (ConfigDetail configDetail in result)
        {
            await GetItems(configDetail);
        }
        return result.ToList();
    }


}
