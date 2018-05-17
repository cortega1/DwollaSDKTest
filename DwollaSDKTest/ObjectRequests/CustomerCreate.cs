using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwollaSDKTest.ObjectRequests
{
    public class CustomerCreate
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string type { get; set; }
        public string address1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string dateOfBirth { get; set; }
        public string ssn { get; set; }
    }
}
