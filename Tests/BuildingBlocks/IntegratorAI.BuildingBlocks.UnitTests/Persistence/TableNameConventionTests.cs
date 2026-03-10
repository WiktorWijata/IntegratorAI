using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using IntegratorAI.BuildingBlocks.Persistence.Conventions;
using NSubstitute;

namespace IntegratorAI.BuildingBlocks.UnitTests.Persistence;

public class TableNameConventionTests
{
    private class EntityA { public int Id { get; set; } }
    private class EntityB { public int Id { get; set; } }
    private class CustomerOrder { public int Id { get; set; } }

    [Fact]
    public void ProcessModelFinalizing_SetsTableNameToClrTypeName()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<EntityA>();
        var conventionModel = (IConventionModel)modelBuilder.Model;
        var contextMock = Substitute.For<IConventionContext<IConventionModelBuilder>>();
        var convention = new TableNameConvention();

        convention.ProcessModelFinalizing(conventionModel.Builder, contextMock);

        Assert.Equal(nameof(EntityA), conventionModel.FindEntityType(typeof(EntityA))!.GetTableName());
    }

    [Fact]
    public void ProcessModelFinalizing_HandlesMultipleEntityTypes()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<EntityA>();
        modelBuilder.Entity<EntityB>();
        var conventionModel = (IConventionModel)modelBuilder.Model;
        var contextMock = Substitute.For<IConventionContext<IConventionModelBuilder>>();
        var convention = new TableNameConvention();

        convention.ProcessModelFinalizing(conventionModel.Builder, contextMock);

        Assert.Equal(nameof(EntityA), conventionModel.FindEntityType(typeof(EntityA))!.GetTableName());
        Assert.Equal(nameof(EntityB), conventionModel.FindEntityType(typeof(EntityB))!.GetTableName());
    }

    [Fact]
    public void ProcessModelFinalizing_OverridesConventionLevelTableName()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<CustomerOrder>();
        var conventionModel = (IConventionModel)modelBuilder.Model;
        conventionModel.FindEntityType(typeof(CustomerOrder))!.SetTableName("customer_orders");
        var contextMock = Substitute.For<IConventionContext<IConventionModelBuilder>>();
        var convention = new TableNameConvention();

        convention.ProcessModelFinalizing(conventionModel.Builder, contextMock);

        Assert.Equal(nameof(CustomerOrder), conventionModel.FindEntityType(typeof(CustomerOrder))!.GetTableName());
    }
}
