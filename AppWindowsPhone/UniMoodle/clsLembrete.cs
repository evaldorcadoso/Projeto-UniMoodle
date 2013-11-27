using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniMoodle
{
    public class clsLembrete
    {
        
        public string name { get; set; }
        public string date { get; set; }
        public string title { get; set; }

        public clsLembrete(string nome, string data,string titulo)
        {
            this.name = nome;
            this.date = data;
            this.title = titulo;
        }
        public clsLembrete()
        {
        }

    }
}
