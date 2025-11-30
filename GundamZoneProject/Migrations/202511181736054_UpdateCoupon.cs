namespace GundamZoneProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateCoupon : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Coupons", "StartDate", c => c.DateTime());
            AlterColumn("dbo.Coupons", "EndDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Coupons", "EndDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Coupons", "StartDate", c => c.DateTime(nullable: false));
        }
    }
}
