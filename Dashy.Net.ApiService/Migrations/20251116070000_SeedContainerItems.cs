using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dashy.Net.Shared.Migrations
{
    /// <summary>
    /// Data migration: creates container (section-container) items for every existing section and
    /// re-parents root items under the created container item. Non-destructive schema-wise.
    /// </summary>
    public partial class SeedContainerItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
-- Create a container item per section missing one
WITH sections_without_container AS (
    SELECT s.""Id"", s.""Name"", s.""Icon""
    FROM ""Sections"" s
    WHERE NOT EXISTS (
        SELECT 1 FROM ""Items"" i WHERE i.""SectionId"" = s.""Id"" AND i.""Widget"" = 'section-container'
    )
)
INSERT INTO ""Items"" (""Title"", ""Icon"", ""Widget"", ""SectionId"", ""Position"")
SELECT swc.""Name"", swc.""Icon"", 'section-container', swc.""Id"",
       COALESCE((SELECT MAX(i.""Position"") FROM ""Items"" i WHERE i.""SectionId"" = swc.""Id""), -1) + 1
FROM sections_without_container swc;

-- Re-parent existing items under the container (skip containers themselves)
UPDATE ""Items"" AS child
SET ""ParentItemId"" = container.""Id""
FROM ""Items"" AS container
WHERE child.""SectionId"" = container.""SectionId""
  AND container.""Widget"" = 'section-container'
  AND child.""ParentItemId"" IS NULL
  AND child.""Id"" <> container.""Id"";
";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = @"
-- Un-parent items first
UPDATE ""Items"" SET ""ParentItemId"" = NULL
WHERE ""ParentItemId"" IN (SELECT ""Id"" FROM ""Items"" WHERE ""Widget"" = 'section-container');
-- Remove container items
DELETE FROM ""Items"" WHERE ""Widget"" = 'section-container';
";
            migrationBuilder.Sql(sql);
        }
    }
}