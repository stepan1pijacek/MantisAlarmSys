using System;
using System.Collections.Generic;
using System.Text;

namespace MantisAlarmSys
{
    public class PropertiesUserList
    {
        public string email { get; set; }
        public int Id { get; set; }
        public string username { get; set; }
        
    }

    public class PropertiesIssueList
    {
        public int handlerId { get; set; }
        public int bugId { get; set; }
        public long fieldName { get; set; }
        public int newValue { get; set; }
        public int user_id { get; set; }
       
    }

    public class EmailProperties
    {
        public string contractingAuthority { get; set; }
        public string email { get; set; }
        public int IssueID { get; set; }
        public DateTime Date { get; set; }
        public string handlerName { get; set; }
        public int status { get; set; }
    }
}
