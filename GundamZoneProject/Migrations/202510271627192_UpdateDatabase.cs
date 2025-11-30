namespace GundamZoneProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tb_Advertise", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Advertise", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Category", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Category", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_News", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_News", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Post", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Post", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Contact", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Contact", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Order", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Order", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Product", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_Product", "ModifiedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_ProductCategory", "CreatedDate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.tb_ProductCategory", "ModifiedDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tb_ProductCategory", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_ProductCategory", "CreatedDate", c => c.String());
            AlterColumn("dbo.tb_Product", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_Product", "CreatedDate", c => c.String());
            AlterColumn("dbo.tb_Order", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_Order", "CreatedDate", c => c.String());
            AlterColumn("dbo.tb_Contact", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_Contact", "CreatedDate", c => c.String());
            AlterColumn("dbo.tb_Post", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_Post", "CreatedDate", c => c.String());
            AlterColumn("dbo.tb_News", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_News", "CreatedDate", c => c.String());
            AlterColumn("dbo.tb_Category", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_Category", "CreatedDate", c => c.String());
            AlterColumn("dbo.tb_Advertise", "ModifiedDate", c => c.String());
            AlterColumn("dbo.tb_Advertise", "CreatedDate", c => c.String());
        }
    }
}
