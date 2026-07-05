using System;
using System.Collections.Generic;
using System.Linq;

namespace ETLVentas.Data.Result
{
    public class EtlReport
    {
        public DateTime GlobalStartTime { get; set; }
        public DateTime GlobalEndTime { get; set; }
        public TimeSpan GlobalDuration => GlobalEndTime - GlobalStartTime;
        
        public List<EntityProcessResult> EntityResults { get; set; } = new List<EntityProcessResult>();

        public void AddResult(EntityProcessResult result)
        {
            EntityResults.Add(result);
        }
        
        public int TotalProcessed => EntityResults.Sum(x => x.TotalRecords);
        public int TotalInserted => EntityResults.Sum(x => x.InsertedRecords);
        public int TotalRejected => EntityResults.Sum(x => x.RejectedRecords);
        public int TotalErrors => EntityResults.Sum(x => x.Errors);
    }
}
