using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FoodHub.Models
{
    public class DeliveryPerson
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [BindNever]        // Ignore binding from form
        [ScaffoldColumn(false)]  // Prevent editor templates from trying to render it
        public string Id { get; set; }  // can be null initially

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string NIC { get; set; }

        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public bool FingerprintEnabled { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}


// using System;
// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;
// using Microsoft.AspNetCore.Mvc.ModelBinding;

// namespace FoodHub.Models
// {
//     public class DeliveryPerson
//     {
//         [Key]
//         [BindNever]
//         public string Id { get; set; }

//         [Required]
//         [MaxLength(100)]
//         public string Name { get; set; }

//         [Required]
//         [MaxLength(100)]
//         [EmailAddress]
//         public string Email { get; set; }

//          [Required]
//         [MaxLength(20)]
//         public string NIC { get; set; }


//         [Required]
//         [MaxLength(255)]
//         public string Password { get; set; }

//         [MaxLength(20)]
//         public string PhoneNumber { get; set; }

//         // Is the delivery person currently active?
//         public bool IsActive { get; set; } = true;

//         // Can the delivery person use fingerprint login?
//         public bool FingerprintEnabled { get; set; } = false;

//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
//         public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
//     }
// }
