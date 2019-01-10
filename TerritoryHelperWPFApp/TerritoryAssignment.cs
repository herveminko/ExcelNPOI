using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerritoryHelperWPFApp
{
    class TerritoryAssignment
    {
        public string TerritoryNumber { get; set; }
        public string TerritoryType { get; set; }
        public string TerritoryDescription { get; set; }

        public string PublisherName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        
        public DateTime? AssignDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Remarks { get; set; }


    }
}
