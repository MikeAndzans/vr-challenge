using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vr_challenge.Data
{
    [Table("ShipmentBox", Schema = "shipment_data")]
    [Index(nameof(BoxId))]
    public class ShipmentBox
    {
        [Key]
        public string BoxId { get; set; }
        public string SupplierId { get; set; }

        [DeleteBehavior(DeleteBehavior.Cascade)]
        public List<ProductShipment> BoxContents { get; set; }
    }
}
