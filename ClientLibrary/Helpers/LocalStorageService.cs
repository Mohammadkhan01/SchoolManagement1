using System;
using Blazored.LocalStorage;

namespace ClientLibrary.Helpers
{
	public class LocalStorageService
	{
		private ILocalStorageService _localStorageService;
		public LocalStorageService(ILocalStorageService local)
        {
			_localStorageService = local;
		}

        private const string Storagekey = "authenticaiton-token";
		public async Task<string> GetToken() => await _localStorageService.GetItemAsStringAsync(Storagekey);
		public async Task SetToken(string item) => await _localStorageService.SetItemAsStringAsync(Storagekey,item);
		public async Task RemoveToken() => await _localStorageService.RemoveItemAsync(Storagekey);


    }
}

