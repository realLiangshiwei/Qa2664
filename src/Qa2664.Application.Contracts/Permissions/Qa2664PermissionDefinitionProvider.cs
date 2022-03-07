using Qa2664.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Qa2664.Permissions;

public class Qa2664PermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(Qa2664Permissions.GroupName);
        //Define your own permissions here. Example:
        //myGroup.AddPermission(Qa2664Permissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<Qa2664Resource>(name);
    }
}
