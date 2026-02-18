using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class RedirectUriList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "RedirectUriList",
                table: "Clients",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

                  migrationBuilder.Sql("""                                                                                                                                                                                                                                                            
UPDATE Clients                                                                                                                                                                                                                                                                  
SET RedirectUriList = CASE                                                                                                                                                                                                                                                      
WHEN RedirectUri IS NOT NULL AND RedirectUri != ''                                                                                                                                                                                                                          
THEN '["' || REPLACE(REPLACE(REPLACE(RedirectUri, '\', '\\'), '"', '\"'), '''', '\''') || '"]'                                                                                                                                                                          
ELSE '[]'                                                                                                                                                                                                                                                                   
END                                                                                                                                                                                                                                                                             
""");

                  migrationBuilder.DropColumn(
                      name: "RedirectUri",
                      table: "Clients");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "RedirectUri",
                table: "Clients",
                type: "TEXT",
                nullable: true);



            migrationBuilder.Sql("""                                                                                                                                                                                                                                                            
                                 UPDATE Clients                                                                                                                                                                                                                                                                  
                                 ET RedirectUri = json_extract(RedirectUriList, '$[0]')                                                                                                                                                                                                                         
                                 WHERE RedirectUriList != '[]' AND RedirectUriList IS NOT NULL                                                                                                                                                                                                                   
                                 """);


            migrationBuilder.DropColumn(
                name: "RedirectUriList",
                table: "Clients");
        }
    }
}
