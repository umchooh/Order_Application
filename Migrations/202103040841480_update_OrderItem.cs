namespace Order_Application.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update_OrderItem : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderItems", "ProductPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.OrderItems", "Amount", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderItems", "Amount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.OrderItems", "ProductPrice", c => c.Double(nullable: false));
        }
    }
}
