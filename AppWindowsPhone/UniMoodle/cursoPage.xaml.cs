using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Runtime.Serialization.Json;

namespace UniMoodle
{
    public partial class cursoPage : PhoneApplicationPage
    {
        #region atributos
        /// <summary>
        /// id do curso
        /// </summary>
        private string id = null;

        #endregion

        #region contrutores

        public cursoPage()
        {
            InitializeComponent();
        }

        #endregion

        #region métodos

        /// <summary>
        /// pegar as aulas
        /// </summary>
        /// <param name="idCurso">id do curso</param>
        private void pegarAulas(string idCurso)
        {
            try
            {
                txtbVazio.Text = "Atualizando...";
                WebClient client = new WebClient();
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                client.DownloadStringAsync(new Uri((Application.Current as App).local + "index.php?funcao=pegar_aulas&token=" + (Application.Current as App).token + "&idcurso=" + idCurso + "&nocache=" + Environment.TickCount));
            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbVazio.Text = "Erro ao atualizar.";
            }

        }

        /// <summary>
        /// animar a progressBar
        /// </summary>
        /// <param name="animar">'True' para animar ou 'False' para parar</param>
        private void animaProgressBar(Boolean animar)
        {
            barraProgresso2.IsIndeterminate = animar;
            if (animar)
            {
                barraProgresso2.Visibility = Visibility.Visible;
            }
            else
            {
                barraProgresso2.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region eventos
        
        //quando a página é carregada
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            animaProgressBar(true);
            pegarAulas(id);
            
        }

        //quando o retorno do webservice estiver pronto(aulas)
        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                animaProgressBar(false);
                txtbVazio.Text = "Sem resposta do servidor!";
                MessageBox.Show("Ops! Parece que o servidor não está querendo funcionar! Por favor, tente novamente...");
                return;
            }
            else
            {
                try
                {
                    string json = e.Result;
                    Stream stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(json));
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<clsAulas>));
                    List<clsAulas> listAulas;
                    listAulas = (List<clsAulas>)serializer.ReadObject(stream);
                    stream.Close();
                    List<clsAulas> listaDeAulas = new List<clsAulas>();
                    if (lstAulas.Items.Count > 0)
                    {
                        lstAulas.ItemsSource = null;
                    }
                    foreach (clsAulas aula in listAulas)
                    {
                        if (aula.name == "")
                            aula.name = "Sem descrição...";
                        if (aula.intro == "")
                            aula.intro = "Sem descrição...";
                        listaDeAulas.Add(new clsAulas(aula.id, aula.name, aula.intro));
                    }
                    lstAulas.ItemsSource = listaDeAulas;
                    if (listaDeAulas.Count == 0)
                    {
                        txtbVazio.Text = "Sem aulas...";
                    }
                    else
                        txtbVazio.Text = "Minhas Aulas:";
                    //pegarAtividades(this.id);
                    animaProgressBar(false);
                }
                catch (Exception e2)
                {
                    animaProgressBar(false);
                    txtbVazio.Text = "Erro ao buscar as aulas";
                    MessageBox.Show("Ops, não foi possível buscar as aulas.");
                }
            }

        }

        //ao navegar para esta página
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            IDictionary<string, string> param =
                this.NavigationContext.QueryString;
            if (param.ContainsKey("id"))
            {
                this.id = param["id"].ToString();
            }
            if (param.ContainsKey("nomecurso"))
            {
                nomeCurso.Text = param["nomecurso"].ToString();
            }
            base.OnNavigatedTo(e);
        }

        //clique em alguma aula
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string idAula = (string)((Button)sender).Tag;
            string intro = (string)((Button)sender).CommandParameter;
            string destino = "/aulaPage.xaml?id=" + idAula + "&intro=" + intro + "&idcurso=" + id;
            this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
        }

        #endregion
    }
}