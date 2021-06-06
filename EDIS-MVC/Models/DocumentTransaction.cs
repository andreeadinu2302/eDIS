using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS_MVC.Models
{
    [Serializable]
    public class DocumentTransaction
    {
        [JsonProperty]
        public string OrganizationName { get; set; }

        [JsonProperty]
        public string DocumentType { get; set; }

        [JsonProperty]
        public string DocumentSerial { get; set; }

        [JsonProperty]
        public string Faculty { get; set; }

        [JsonProperty]
        public string Title { get; set; }

        [JsonProperty]
        public string LearningType { get; set; }

        [JsonProperty]
        public string Average { get; set; }

        [JsonProperty]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMMM/yyyy}")]

        public DateTime DateOfIssue { get; set; }

        [JsonProperty]
        public string HolderName { get; set; }

    }
}
