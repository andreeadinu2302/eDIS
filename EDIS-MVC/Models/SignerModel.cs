using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS_MVC.Models
{
    public class SignerModel
    {
        public List<String> public_keys { get; set; }
        public Condition condition { get; set; }
    }

    public class Details
    {
        public string type { get; set; }
        public string public_key { get; set; }
    }

    public class Condition
    {
        public Details details { get; set; }
        public string uri { get; set; }
    }
}
