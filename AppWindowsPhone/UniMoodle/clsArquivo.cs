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
    public class clsArquivo
    {
        public string id { get; set; }
        public string filename { get; set; }
        public string contenthash{get;set;}

        public clsArquivo(string idArquivo, string nomeArquivo, string conteudoArquivo)
        {
            id = idArquivo;
            filename = nomeArquivo;
            contenthash = conteudoArquivo;
        }

        public clsArquivo()
        {
        }

        

    }
}
