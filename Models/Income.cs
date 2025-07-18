using System;
using System.ComponentModel.DataAnnotations;

namespace test_project.Models
{
    public class Income
    {
        public int Id { get; set; }


        public string Source { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public int UserId { get; set; }

        public Demo_user User { get; set; }
    }
}
