using System;

namespace WebApplication1.Data
{
    public class DailyStatistic : BaseEntity
    {
        public int? PatientId { get; set; }
        public Patient Patient { get; set; }
        public DateTime Date { get; set; }
        public int SleptTimeInMinutes { get; set; }
        public double Ahi { get; set; }
    }
}
