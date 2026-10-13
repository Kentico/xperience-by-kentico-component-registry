using Kentico.Xperience.Admin.Base;

using Kentico.Xperience.Labs.ComponentRegistry.Admin;

[assembly: CMS.RegisterModule(typeof(ComponentRegistryWebAdminModule))]

namespace Kentico.Xperience.Labs.ComponentRegistry.Admin
{
    internal class ComponentRegistryWebAdminModule : AdminModule
    {

        // Pre-rename module name kept for stable module identity
        public ComponentRegistryWebAdminModule() : base("Kentico.Xperience.ComponentRegistry.Admin") { }

        protected override void OnInit()
        {
            base.OnInit();

            RegisterClientModule("kentico", "xperience-labs-component-registry-admin");
        }
    }
}
