namespace Sample.Api.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;


    public class OrderViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string CustomerNumber { get; set; }

        [Required]
        public string PaymentCardNumber { get; set; }

        public string Notes { get; set; }
    }
}