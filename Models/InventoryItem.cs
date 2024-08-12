namespace StockTracker.Client.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Brand is required")]
        public string Brand { get; set; }
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }
        [Required(ErrorMessage = "Model is required")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Serial Number is required")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "Recovery Date is required")]
        public DateTime RecoveryDate { get; set; }
        [Required(ErrorMessage = "Verification Date is required")]
        public DateTime? VerificationDate { get; set; }

        [Required(ErrorMessage = "VerifiedBy by is required")]
        public string VerifiedBy { get; set; }
        

        public string Description { get; set; }
        public State State { get; set; }
    }
}
