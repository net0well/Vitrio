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
        builder.Property(category => category.ProfessionalSingular).HasMaxLength(Category.ProfessionalMaxLength);
        builder.Property(category => category.ProfessionalPlural).HasMaxLength(Category.ProfessionalMaxLength);
        builder.HasData(
            Category.Seed(1, "eletrica", "Elétrica", "bolt", "eletricista", "eletricistas"),
            Category.Seed(2, "hidraulica", "Hidráulica", "droplet", "encanador", "encanadores"),
            Category.Seed(3, "pedreiro", "Pedreiro", "brick", "pedreiro", "pedreiros"),
            Category.Seed(4, "pintura", "Pintura", "roller", "pintor", "pintores"),
            Category.Seed(5, "diarista", "Diarista", "broom", "diarista", "diaristas"),
            Category.Seed(6, "jardinagem", "Jardinagem", "leaf", "jardineiro", "jardineiros"),
            Category.Seed(7, "montagem-de-moveis", "Montagem de móveis", "wrench", "montador de móveis", "montadores de móveis"),
            Category.Seed(8, "ar-condicionado", "Ar-condicionado", "sliders", "técnico de ar-condicionado", "técnicos de ar-condicionado"),
            Category.Seed(9, "marido-de-aluguel", "Marido de aluguel", "wrench", "marido de aluguel", "maridos de aluguel"),
            Category.Seed(10, "fretes-e-mudancas", "Fretes e mudanças", "transfer", "freteiro", "freteiros"),
            Category.Seed(11, "beleza-a-domicilio", "Beleza a domicílio", "star", "profissional de beleza", "profissionais de beleza"),
            Category.Seed(12, "aulas-particulares", "Aulas particulares", "message", "professor particular", "professores particulares"),
            Category.Seed(13, "informatica", "Informática", "grid", "técnico de informática", "técnicos de informática"),
            Category.Seed(14, "cuidador", "Cuidador(a)", "user", "cuidador", "cuidadores"),
            Category.Seed(15, "pet", "Pet", "heart", "profissional para pet", "profissionais para pet"));
    }
}
