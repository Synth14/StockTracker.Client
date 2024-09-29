namespace StockTracker.Client.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de l'acheteur/preneur est requis")]
        [StringLength(255, ErrorMessage = "Le nom ne peut pas dépasser 255 caractères")]
        public string BuyerName { get; set; }

        [Required(ErrorMessage = "La date de la commande est requise")]
        public DateTime Date { get; set; }

        [StringLength(255, ErrorMessage = "Le nom du créateur ne peut pas dépasser 255 caractères")]
        public string CreatedBy { get; set; }

        public List<InventoryItem> Items { get; set; } = new List<InventoryItem>();

        // Propriété calculée pour obtenir le nombre total d'articles
        public int TotalItems => Items?.Count ?? 0;

        // Méthode pour ajouter un article à la commande
        public void AddItem(InventoryItem item)
        {
            if (Items == null)
            {
                Items = new List<InventoryItem>();
            }
            Items.Add(item);
        }

        // Méthode pour retirer un article de la commande
        public void RemoveItem(InventoryItem item)
        {
            Items?.Remove(item);
        }

        // Override de ToString pour l'affichage et le logging
        public override string ToString()
        {
            return $"Order {Id}: {BuyerName}, Date: {Date}, Items: {TotalItems}";
        }
    }
}