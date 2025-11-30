namespace GundamZoneProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCouponOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Order", "DiscountAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.tb_Order", "CouponCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tb_Order", "CouponCode");
            DropColumn("dbo.tb_Order", "DiscountAmount");
        }
    }
}
