using System;
using System.Threading.Tasks;

using KeyboardSwitch.Common.Services;

namespace KeyboardSwitch.Linux.Services
{
    public sealed class ClipboardTextService : ITextService
    {
        public Task<string?> GetTextAsync() =>
            throw new NotImplementedException();

        public Task SetTextAsync(string text) =>
            throw new NotImplementedException();
    }
}
