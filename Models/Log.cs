using System;
using System.ComponentModel.DataAnnotations;

namespace MVCCoreVue.Models
{
    public class Log
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        [Required, MaxLength(50)]
        public string Level { get; set; }

        public string Message { get; set; }

        [MaxLength(250)]
        public string Logger { get; set; }

        public string Callsite { get; set; }

        public string Exception { get; set; }

        public string Url { get; set; }
    }
}
