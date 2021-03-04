namespace Order_Application.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class order_application_setup_3 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.OrderItems", "OrderItemID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OrderItems", "OrderItemID", c => c.Int(nullable: false));
        }
    }
}
