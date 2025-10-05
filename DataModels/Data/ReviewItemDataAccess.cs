using DataModels.DataTools;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels.Data;

public class ReviewItemDataAccess(string connectionString) : DataAccess<ReviewItem>(connectionString, ReviewItem.Metadata)
{
    public async Task<ReviewItem?> GetByReviewPointAndProjectId(int ReviewPointId, int ProjectId)
    {
        var sql = "SELECT * FROM ReviewItem WHERE ProjectId = @ProjectId AND ReviewPointId = @ReviewPointId";
        return await QueryFirstOrDefaultAsync(sql, new { ProjectId, ReviewPointId });
    }
}
