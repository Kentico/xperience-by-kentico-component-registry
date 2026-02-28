using Kentico.Xperience.Admin.Base;

using Kentico.Xperience.ComponentRegistry.Admin;

[assembly: CMS.RegisterModule(typeof(ComponentRegistryWebAdminModule))]

namespace Kentico.Xperience.ComponentRegistry.Admin
{
    internal class ComponentRegistryWebAdminModule : AdminModule
    {

        public ComponentRegistryWebAdminModule() : base("Kentico.Xperience.ComponentRegistry.Admin") { }

        protected override void OnInit()
        {
            base.OnInit();

            RegisterClientModule("kentico", "xperience-integrations-component-registry-web-admin");
        }
    }
}
