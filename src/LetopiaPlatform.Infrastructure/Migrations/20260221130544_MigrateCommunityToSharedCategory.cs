using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LetopiaPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateCommunityToSharedCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Seed categories from existing community topic_category values
            migrationBuilder.Sql(@"
                INSERT INTO categories (id, name, slug, icon_url, type)
                SELECT DISTINCT
                    gen_random_uuid(),
                    topic_category,
                    lower(replace(replace(topic_category, ' ', '-'), '.', '')),
                    NULL,
                    'Community'
                FROM communities
                WHERE topic_category IS NOT NULL
                ON CONFLICT DO NOTHING;
            ");

            // 2. Add category_id column (nullable first)
            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "communities",
                type: "uuid",
                nullable: true);
            
            // 3. Populate category_id from seeded data
            migrationBuilder.Sql(@"
                UPDATE communities c
                SET category_id = cat.id
                FROM categories cat
                WHERE c.topic_category = cat.name AND cat.type = 'Community';
            ");

            // 4. Make category_id non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "category_id",
                table: "communities",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
            
            // 5. Drop old column and its index
            migrationBuilder.DropIndex(
                name: "ix_communities_topic_category",
                table: "communities");
            migrationBuilder.DropColumn(
                name: "topic_category", "communities");
            
            // 6. Add FK and new index
            migrationBuilder.CreateIndex(
                name: "ix_communities_category_id",
                table: "communities",
                column: "category_id");
            
            migrationBuilder.AddForeignKey(
                name: "FK_communities_categories_category_id",
                table: "communities",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse: add topic_category back, populate from category, drop FK
            migrationBuilder.AddColumn<string>(
                name: "topic_category",
                table: "communities",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE communities c
                SET topic_category = cat.name
                FROM categories cat
                WHERE cat.id = c.category_id;
            ");

            migrationBuilder.DropForeignKey("FK_communities_categories_category_id", "communities");
            migrationBuilder.DropIndex("ix_communities_category_id", "communities");
            migrationBuilder.DropColumn("category_id", "communities");

            migrationBuilder.CreateIndex(
                name: "ix_communities_topic_category",
                table: "communities",
                column: "topic_category");
        }
    }
}
