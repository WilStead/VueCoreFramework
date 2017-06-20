using System;
using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// A model for log entries.
    /// </summary>
    public class Log
    {
        /// <summary>
        /// The ID (primary key) for the log entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The timestamp of the log entry.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The level of the log entry.
        /// </summary>
        [Required, MaxLength(50)]
        public string Level { get; set; }

        /// <summary>
        /// An optional message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The logger used to create this entry.
        /// </summary>
        [MaxLength(250)]
        public string Logger { get; set; }

        /// <summary>
        /// The site at which the entry was generated.
        /// </summary>
        public string Callsite { get; set; }

        /// <summary>
        /// The exception type which caused the log entry to be cretaed, if any.
        /// </summary>
        public string Exception { get; set; }

        /// <summary>
        /// The URL of the page on which the log entry was generated, if any.
        /// </summary>
        public string Url { get; set; }
    }
}
