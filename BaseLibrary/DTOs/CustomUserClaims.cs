using System;
namespace BaseLibrary.DTOs
{
	public class CustomUserClaims
	{
		public  string _Id;
		public string _Name;
		public string _Email;
		public string _Role;

		public CustomUserClaims(string? Id=null!, string? Name=null!,string? Email=null!,string? Role=null!)
		{
			_Id = Id;
			_Name = Name;
			_Email = Email;
			_Role = Role;
		}
	}
}

