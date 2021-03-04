using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace Order_Application.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public double ProductPrice { get; set; }
        public bool ProductHasPic { get; set; }

        //If the player has an image, record the extension of the image (.png, .gif, .jpg, etc.)
        public string PicExtension { get; set; }
        public ICollection<OrderItems> OrderItems { get; set; }
    }

    public class ProductDto
    {
        public int ProductID { get; set; }
        [DisplayName("Product Name")]
        public string ProductName { get; set; }
        [DisplayName("Product Description")]
        public string ProductDescription { get; set; }
        [DisplayName("Product Price")]
        public double ProductPrice { get; set; }

        public bool ProductHasPic { get; set; }
        public string PicExtension { get; set; }
    }
}