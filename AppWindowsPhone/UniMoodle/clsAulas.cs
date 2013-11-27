using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace UniMoodle
{
    public class clsAulas
    {
        public string id { get; set; }
        public string name { get; set; }
        public string intro { get; set; }

        public clsAulas()
        {
        }

        public clsAulas(string idAula, string nomeAula, string introAula)
        {
            this.id = idAula;
            this.name = nomeAula;
            this.intro = introAula;
        }
    }

    
}
