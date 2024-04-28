using System;
namespace BaseLibrary.Entities
{
	public class Transaction
	{
		public int TransactionId { get; set; }
		public StudentData StudentData { get; set; }
		public int StudentDataId { get; set; }
		public int FeesId { get; set; }
		public int Amount { get; set; }
		public string Remark { get; set; }

	}
}

