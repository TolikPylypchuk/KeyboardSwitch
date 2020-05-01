using System.Threading.Tasks;

using KeyboardSwitch.Settings.Core.Models;

namespace KeyboardSwitch.Settings.Core.ViewModels
{
    public sealed class CharMappingViewModel : FormBase<CharMappingModel, CharMappingViewModel>
    {
        protected override CharMappingViewModel Self
            => this;

        protected override Task<CharMappingModel> OnSaveAsync()
        {
            throw new System.NotImplementedException();
        }

        protected override void CopyProperties()
        {
            throw new System.NotImplementedException();
        }
    }
}
