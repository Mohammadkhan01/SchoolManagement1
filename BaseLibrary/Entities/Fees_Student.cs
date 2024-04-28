using System;
namespace BaseLibrary.Entities
{
	public class Fees_Student
	{
		public Fees Fees { get; set; }
		public int FeesId { get; set; }
		public StudentData StudentData { get; set; }
		public int StudentDataId { get; set; }
		public FeeType FeeType { get; set; }
		public int FeeTypeId { get; set; }
		public DateTime EffectiveFrom { get; set; }
		public DateTime  DueDate{get;set;}
		public int Amount { get; set; }
		public int Discount { get; set; }
		public bool Status { get; set; }
	}
}

