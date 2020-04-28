using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class OtherSettingsViewModel : FormBase<OtherSettingsModel, OtherSettingsViewModel>
    {
        protected override OtherSettingsViewModel Self
            => this;

        protected override Task<OtherSettingsModel> OnSaveAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override void CopyProperties()
        {
            throw new System.NotImplementedException();
        }
    }
}
