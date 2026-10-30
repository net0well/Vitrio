using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Categories;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();
        builder.Property(category => category.Slug).HasMaxLength(Category.SlugMaxLength);
        builder.HasIndex(category => category.Slug).IsUnique();
        builder.Property(category => category.Name).HasMaxLength(Category.NameMaxLength);
        builder.Property(category => category.Icon).HasMaxLength(Category.IconMaxLength);
        builder.HasData(
            Category.Seed(1, "eletrica", "Elétrica", "bolt"),
            Category.Seed(2, "hidraulica", "Hidráulica", "droplet"),
            Category.Seed(3, "pedreiro", "Pedreiro", "brick"),
            Category.Seed(4, "pintura", "Pintura", "roller"),
            Category.Seed(5, "diarista", "Diarista", "broom"),
            Category.Seed(6, "jardinagem", "Jardinagem", "leaf"),
            Category.Seed(7, "montagem-de-moveis", "Montagem de móveis", "wrench"),
            Category.Seed(8, "ar-condicionado", "Ar-condicionado", "sliders"),
            Category.Seed(9, "marido-de-aluguel", "Marido de aluguel", "wrench"),
            Category.Seed(10, "fretes-e-mudancas", "Fretes e mudanças", "transfer"),
            Category.Seed(11, "beleza-a-domicilio", "Beleza a domicílio", "star"),
            Category.Seed(12, "aulas-particulares", "Aulas particulares", "message"),
            Category.Seed(13, "informatica", "Informática", "grid"),
            Category.Seed(14, "cuidador", "Cuidador(a)", "user"),
            Category.Seed(15, "pet", "Pet", "heart"));
    }
}
