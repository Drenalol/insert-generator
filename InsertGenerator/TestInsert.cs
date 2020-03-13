using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InsertGenerator
{
    [Table("test")]
    public class TestInsert
    {
        public int Id { get; set; }
        public string NotIgnored { get; set; }
        [InsertColumnIgnore]
        public string Ignored { get; set; }
        public DateTime DateTime { get; set; }
        public string[] Instructions { get; set; }
    }
}