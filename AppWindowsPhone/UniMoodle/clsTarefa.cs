using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniMoodle
{
    public class clsTarefa
    {
        public string id { get; set; }
        public string intro { get; set; }
        public string duedate { get; set; }
        public string fullname { get; set; }
        public string maxbytes { get; set; }

        public clsTarefa(string idTarefa, string introTarefa, string duedateTarefa, string fullnameTarefa, string bytesTarefa)
        {
            this.id=idTarefa;
            this.intro=introTarefa;
            this.duedate=duedateTarefa;
            this.fullname= fullnameTarefa;
            this.maxbytes= bytesTarefa;
        }

        public clsTarefa()
        {
        }
    }
}
