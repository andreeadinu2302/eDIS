using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS_MVC.Models
{
    public class Organization
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [NotMapped]
        [DisplayName("Upload Logo")]
        public IFormFile Logo { get; set; }

        public string LogoName { get; set; }

        public byte[] PublicKey { get; set; }

        public byte[] PrivateKey { get; set; }

        public string TransactionId { get; set; }

        public string Domain { get; set; }

        public string AdminUserId { get; set; }

    }
}
