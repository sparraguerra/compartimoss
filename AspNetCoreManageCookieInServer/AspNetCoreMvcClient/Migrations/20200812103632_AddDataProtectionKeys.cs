using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AspNetCoreMvcClient.Migrations
{
    public partial class AddDataProtectionKeys : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthenticationTicket",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    Value = table.Column<byte[]>(nullable: true),
                    LastActivity = table.Column<DateTimeOffset>(nullable: true),
                    Expires = table.Column<DateTimeOffset>(nullable: true),
                    RemoteIpAddress = table.Column<string>(nullable: true),
                    OperatingSystem = table.Column<string>(nullable: true),
                    UserAgentFamily = table.Column<string>(nullable: true),
                    UserAgentVersion = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationTicket", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FriendlyName = table.Column<string>(nullable: true),
                    Xml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationTicket");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");
        }
    }
}
