using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class PreferencesViewModel : FormBase<PreferencesModel, PreferencesViewModel>
    {
        protected override PreferencesViewModel Self
            => this;

        protected override Task<PreferencesModel> OnSaveAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override void CopyProperties()
        {
            throw new System.NotImplementedException();
        }
    }
}
