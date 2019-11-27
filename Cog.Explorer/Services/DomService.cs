using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SIL.Cog.Explorer.Services
{
	public class DomService
	{
		private readonly IJSRuntime _jsRuntime;

		public DomService(IJSRuntime jsRuntime)
		{
			_jsRuntime = jsRuntime;
		}

		public async Task FocusElementAsync(ElementReference elementRef)
		{
			await _jsRuntime.InvokeVoidAsync("DomInterop.focusElement", elementRef);
		}
	}
}
