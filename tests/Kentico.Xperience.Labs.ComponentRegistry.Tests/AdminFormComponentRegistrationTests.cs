using Microsoft.Extensions.DependencyInjection;

using AdminRegisterFormComponentAttribute = Kentico.Xperience.Admin.Base.Forms.RegisterFormComponentAttribute;
using BuilderRegisterFormComponentAttribute = Kentico.Forms.Web.Mvc.RegisterFormComponentAttribute;

[assembly: AdminRegisterFormComponent("Acme.Admin.Input", typeof(string), "Admin input")]
[assembly: AdminRegisterFormComponent("Acme.Admin.Other", typeof(int), "Admin other")]
[assembly: BuilderRegisterFormComponent("Acme.Builder.Input", typeof(Uri), "Builder input")]

namespace Kentico.Xperience.Labs.ComponentRegistry.Tests;

public class AdminFormComponentRegistrationTests
{
    [Test]
    public void AddComponentRegistry_KeepsAdminAndFormBuilderRegistrationsSeparate()
    {
        var services = new ServiceCollection().AddComponentRegistry([typeof(AdminFormComponentRegistrationTests).Assembly]);
        using var provider = services.BuildServiceProvider();

        var adminStore = provider.GetRequiredService<IComponentDefinitionStore<AdminFormComponentDefinition>>();
        var builderStore = provider.GetRequiredService<IComponentDefinitionStore<FormBuilderComponentDefinition>>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(adminStore.GetAll().Select(d => d.Identifier), Is.EquivalentTo(new[] { "Acme.Admin.Input", "Acme.Admin.Other" }));
            Assert.That(builderStore.GetAll().Select(d => d.Identifier), Is.EquivalentTo(new[] { "Acme.Builder.Input" }));
            Assert.That(adminStore.Get("acme.admin.input")?.MarkedType, Is.EqualTo(typeof(string)));
            Assert.That(adminStore.Get("Acme.Admin.Other")?.IsAvailable, Is.True);
        }
    }
}
