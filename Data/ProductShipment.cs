using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vr_challenge.Data
{
    [Table("ProductShipment", Schema = "shipment_data")]
    [Index(nameof(ProductShipmentId))]
    [Index(nameof(BoxId))]
    public class ProductShipment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductShipmentId { get; set; }
        public string Barcode { get; set; }
        public string PoNumber { get; set; }
        public int Quantity { get; set; }

        public string BoxId { get; set; }
        public ShipmentBox ShipmentBox { get; set; }
    }
}
