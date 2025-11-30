namespace GundamZoneProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateManufacturer : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Product", "Manufacturer", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tb_Product", "Manufacturer");
        }
    }
}
