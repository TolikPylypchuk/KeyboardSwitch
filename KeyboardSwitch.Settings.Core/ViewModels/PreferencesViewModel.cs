using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class PreferencesViewModel : FormBase<PreferencesModel, PreferencesViewModel>
    {
        public PreferencesViewModel(PreferencesModel preferencesModel)
        {
            this.PreferencesModel = preferencesModel;

            this.CopyProperties();
            this.EnableChangeTracking();
        }

        public PreferencesModel PreferencesModel { get; }

        protected override PreferencesViewModel Self
            => this;

        protected override Task<PreferencesModel> OnSaveAsync()
        {
            return Task.FromResult(this.PreferencesModel);
        }

        protected override void CopyProperties()
        {
        }
    }
}
