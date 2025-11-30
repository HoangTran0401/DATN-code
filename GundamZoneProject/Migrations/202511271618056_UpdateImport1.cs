namespace GundamZoneProject.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateImport1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Imports", "OldStock", c => c.Int(nullable: false));
            AddColumn("dbo.Imports", "NewStock", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Imports", "NewStock");
            DropColumn("dbo.Imports", "OldStock");
        }
    }
}
