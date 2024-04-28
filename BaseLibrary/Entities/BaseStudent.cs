using System;
using System.Text.Json.Serialization;

namespace BaseLibrary.Entities
{
	public class BaseStudent
	{
		
        public int Id { get; set; }
        public string? Name { get; set; }
        [JsonIgnore]
        public List<StudentData>? StudentDatas { get; set; }
    
	}
}

