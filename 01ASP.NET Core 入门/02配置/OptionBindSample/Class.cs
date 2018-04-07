using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OptionBindSample
{
    public class Class
    {
        public int ClassNo { get; set; }
        public string ClassDesc { set; get; }
        public List<Student> Students { get; set; }
    }
    public class Student
    {
        public string Name { get; set; }
        public string Age { get; set; }
    }
}
