using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EDIS_MVC.Models
{
    public class Document
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Diploma type")]
        public string DocumentType { get; set; }

        [Required]
        [Display(Name = "Diploma serial number")]
        public string DocumentSerial { get; set; }

        [Required]
        [Display(Name = "Issuing faculty")]
        public string Faculty { get; set; }

        [Required]
        [Display(Name = "Title offered")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Type of education")]
        public string LearningType { get; set; }

        [Required]
        [Display(Name = "Final average mark")]
        public string Average { get; set; }

        [Required]
        [Display(Name = "Holder's name")]
        public string HolderName { get; set; }

        public string TransactionId { get; set; }

        public int OrganizationId { get; set; }
    }
}
