using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace UniMoodle
{
    public class clsAtividades
    {
        public string id { get; set; }
        public string itemname { get; set; }
        public string courseid { get; set; }
        public string date { get; set; }
        public string nome_curso { get; set; }
        public string prazo { get; set; }

        public clsAtividades(string idAtividade, string nomeAtividade, string idCurso, string dateAtividade, string nomeCurso, string prazoTarefa)
        {
            this.id=idAtividade;
            this.itemname=nomeAtividade;
            this.courseid=idCurso;
            this.date = dateAtividade;
            this.nome_curso = nomeCurso;
            this.prazo = prazoTarefa;
        }

        public clsAtividades()
        {
        }
    }
}
