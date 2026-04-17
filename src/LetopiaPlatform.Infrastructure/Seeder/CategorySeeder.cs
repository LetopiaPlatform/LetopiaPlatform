using LetopiaPlatform.Core.Entities;
using LetopiaPlatform.Core.Enums;
using LetopiaPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LetopiaPlatform.Infrastructure.Seeder;

public static class CategorySeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Categories.AnyAsync())
            return;

        var categories = BuildCommunityCategories();

        dbContext.Categories.AddRange(categories);
        await dbContext.SaveChangesAsync();
    }

    private static List<Category> BuildCommunityCategories()
    {
        var tree = new Dictionary<string, string[]>
        {
            ["Technology"] =
            [
                "Web Development",
                "Game Development",
                "Mobile Apps",
                "Cyber Security",
                "Cloud Computing",
                "Data Science",
                "BlockChain",
                "DevOps",
                "AI & Machine Learning"
            ],
            ["Creative Arts"] =
            [
                "Graphic Design",
                "UI/UX Design",
                "Motion Graphics",
                "Digital Painting",
                "3D Art",
                "Animation",
                "Fashion Design",
                "Filmmaking",
                "Interior Design"
            ],
            ["Business"] =
            [
                "Entrepreneurship",
                "Marketing",
                "Finance",
                "E-Commerce",
                "Real Estate",
                "Human Resources",
                "Sales",
                "Public Relationship",
                "Stock Market",
                "Product Management",
                "Leadership",
            ],
            ["Lifestyle"] =
            [
                "Health & Fitness",
                "Personal Development",
                "Mental Health"
            ],
            ["Science"] =
            [
                "Physics",
                "Mathematics",
                "Biology",
                "Chemistry",
                "Environmental Science"
            ],
            ["Education"] =
            [
                "Language Learning",
                "History",
                "Philosophy",
                "Literature",
                "Academic Writing",
                "Online Teaching",
                "Political Science"
            ]
        };

        var categories = new List<Category>();

        foreach (var (mainName, subNames) in tree)
        {
            var main = new Category
            {
                Name = mainName,
                Slug = ToSlug(mainName),
                Type = CategoryType.Community
            };

            categories.Add(main);

            foreach (var subName in subNames)
            {
                categories.Add(new Category
                {
                    Name = subName,
                    Slug = ToSlug(subName),
                    Type = CategoryType.Community,
                    ParentCategory = main
                });
            }
        }

        return categories;
    }

    private static string ToSlug(string name)
        => name.ToLowerInvariant()
            .Replace("&", "and")
            .Replace(" ", "-")
            .Replace("/", "-");
}
