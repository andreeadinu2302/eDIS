using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS_MVC.Models
{
    [Serializable]
    public class OrganizationTransaction
    {
        [JsonProperty]
        public string OrganizationName { get; set; }

        [JsonProperty]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMMM/yyyy}")]
        public DateTime DateOfCreation { get; set; }

        [JsonProperty]
        public byte[] PublicKey { get; set; }
    }
}
