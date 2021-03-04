namespace Order_Application.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class product_images : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ProductPicExtension", c => c.String());
            DropColumn("dbo.Products", "PicExtension");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Products", "PicExtension", c => c.String());
            DropColumn("dbo.Products", "ProductPicExtension");
        }
    }
}
