using System.Collections.Generic;

namespace WebApplication1.Data
{
    public class Patient : BaseEntity
    {
        public string Name { get; set; }

        public IList<DailyStatistic> DailyStatistics { get; set; }
    }

    public class PatientOnMainPage : BaseEntity
    {
        public string Name { get; set; }
        public int DaysUsed { get; set; }
    }
}