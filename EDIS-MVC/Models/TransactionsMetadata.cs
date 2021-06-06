using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS_MVC.Models
{
    [Serializable]
    public class TransactionsMetadata
    {
        [JsonProperty]
        public string SignerIP { get; set; }
    }
}
