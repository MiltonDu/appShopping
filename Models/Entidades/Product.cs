using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Entidades
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        public List<GroupAttribute> GroupAttributes { get; set; }
    }

    public class GroupAttribute
    {
        [Key]
        public int GroupAttributeId { get; set; }

        [Required]
        public GroupAttributeType GroupAttributeType { get; set; }

        [Required]
        public string Description { get; set; }

        public QuantityInformation QuantityInformation { get; set; }

        public List<Attribute> Attributes { get; set; }

        public int Order { get; set; }
    }

    public class GroupAttributeType
    {
        [Key]
        public int GroupAttributeTypeId { get; set; }

        [Required]
        public string Name { get; set; }
    }

    public class QuantityInformation
    {
        public int GroupAttributeQuantity { get; set; }
        public bool ShowPricePerProduct { get; set; }
        public bool IsShown { get; set; }
        public bool IsEditable { get; set; }
        public bool IsVerified { get; set; }
        public string VerifyValue { get; set; }
    }

    public class Attribute
    {
        [Key]
        public int AttributeId { get; set; }

        [Required]
        public string Name { get; set; }
        public int DefaultQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public decimal PriceImpactAmount { get; set; }
        public bool IsRequired { get; set; }
        public int? NegativeAttributeId { get; set; }
        public int Order { get; set; }
        public string StatusId { get; set; }
        public string UrlImage { get; set; }
    }
}
