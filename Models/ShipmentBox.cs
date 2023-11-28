/*namespace vr_challenge.Models
{
    public class ShipmentBox
    {
        public ShipmentBox(string supplierIdentifier, string identifier)
        {
            SupplierIdentifier = supplierIdentifier;
            Identifier = identifier;
            Contents = new List<Content>();
        }

        public string SupplierIdentifier { get; }
        public string Identifier { get; }
        public List<Content> Contents { get; }


        // TODO: add check for duplicates? 
        // if duplicate - just adjust content quantity or throw error?
        public void AppendContent(Content content)
        {
            Contents.Add(content);
        }

        public void AppendContent(ICollection<Content> contents)
        {
            Contents.AddRange(contents);
        }

        public class Content
        {
            public Content(string poNumber, string isbn, int quantity)
            {
                PoNumber = poNumber;
                Isbn = isbn;
                Quantity = quantity;
            }

            public string PoNumber { get; }
            public string Isbn { get; }
            public int Quantity { get; }
        }
    }
}
*/