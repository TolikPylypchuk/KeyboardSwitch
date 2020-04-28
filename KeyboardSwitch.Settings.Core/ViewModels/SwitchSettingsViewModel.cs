using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class SwitchSettingsViewModel : FormBase<SwitchSettingsModel, SwitchSettingsViewModel>
    {
        protected override SwitchSettingsViewModel Self
            => this;

        protected override Task<SwitchSettingsModel> OnSaveAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override void CopyProperties()
        {
            throw new System.NotImplementedException();
        }
    }
}
