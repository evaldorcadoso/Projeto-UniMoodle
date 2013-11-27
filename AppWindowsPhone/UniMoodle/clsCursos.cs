using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniMoodle
{
    public class clsCursos
    {
        public string id { get; set; }
        public string fullname { get; set; }

        public clsCursos(string idc, string fullnamec)
        {
            this.id = idc;
            this.fullname = fullnamec;
        }

        public clsCursos()
        {
        }
    }
}
