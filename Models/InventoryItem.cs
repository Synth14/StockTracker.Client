namespace StockTracker.Client.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "La marque est requise")]
        public string? Brand { get; set; }
        [Required(ErrorMessage = "Le type est requis")]
        public string? Type { get; set; }
        [Required(ErrorMessage = "Le modèle est requis")]
        public string? Model { get; set; }

        [Required(ErrorMessage = "Le numéro de série est requis")]
        public string? SerialNumber { get; set; }

        [Required(ErrorMessage = "La date de récupération est requise")]
        public DateTime? RecoveryDate { get; set; }
        [Required(ErrorMessage = "La date de vérification est requise")]
        public DateTime? VerificationDate { get; set; }

        [Required(ErrorMessage = "Le champs ajouté par est requis")]
        public string? VerifiedBy { get; set; }
        

        public string Description { get; set; }
        public State State { get; set; }
    }
}
