using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManager.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public required string CustomerName { get; set; }
    }
}
