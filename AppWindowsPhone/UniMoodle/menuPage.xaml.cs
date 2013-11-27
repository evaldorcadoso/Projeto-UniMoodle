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
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Windows.Media.Imaging;
using System.Text;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;





namespace UniMoodle
{
    public partial class menuPage : PhoneApplicationPage
    {
        #region atributos

        /// <summary>
        /// gurda os lembretes da aplicação
        /// </summary>
        IEnumerable<ScheduledNotification> lembretes;

        /// <summary>
        /// guarda a tile da aplicação
        /// </summary>
        ShellTile tile = ShellTile.ActiveTiles.First();

        /// <summary>
        /// obtém o armazenamento local do aplicativo
        /// </summary>
        IsolatedStorageSettings iso = IsolatedStorageSettings.ApplicationSettings;
        
        /// <summary>
        /// local no aplicativo para salvar o arquivo
        /// </summary>
        private IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();

        /// <summary>
        /// lista das atividades
        /// </summary>
        List<clsAtividades> listaDeAtividades = new List<clsAtividades>();

        /// <summary>
        /// lista dos cursos
        /// </summary>
        List<clsCursos> listaDeCursos = new List<clsCursos>();

        #endregion

        #region construtores

        public menuPage()
        {
            InitializeComponent();
        }

        #endregion

        #region métodos

        /// <summary>
        /// animar a progressBar
        /// </summary>
        /// <param name="animar">'True' para animar ou 'False' para parar</param>
        private void animaProgressBar(Boolean animar)
        {
            barraProgresso.IsIndeterminate = animar;
            barraProgresso2.IsIndeterminate = animar;
            if (animar)
            {
                txtbCabecalho.Text = "Atualizando..";
                barraProgresso.Visibility = Visibility.Visible;
                barraProgresso2.Visibility = Visibility.Visible;
            }
            else
            {
                txtbCabecalho.Text = "Próximas Atividades";
                barraProgresso.Visibility = Visibility.Collapsed;
                barraProgresso2.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// chamar o WebService e pegar os cursos
        /// </summary>
        /// <param name="idAluno"></param>
        private void pegarCursos(string idAluno)
        {
            try
            {

                txtbVazio.Text = "Atualizando...";
                WebClient client = new WebClient();
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                client.DownloadStringAsync(new Uri((Application.Current as App).local + "index.php?funcao=pegar_cursos&token=" + (Application.Current as App).token + "&id=" + idAluno + "&nocache=" + Environment.TickCount));
            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbVazio.Text = "Erro ao atualizar";
                MessageBox.Show("Ops, algo deu errado ao buscar os cursos.");
            }

        }

        /// <summary>
        /// chamar o WebService e pegar as atividades
        /// </summary>
        /// <param name="idAluno"></param>
        private void pegarAtividades(string idAluno)
        {
            try
            {
                txtbCabecalho.Text = "Atualizando...";
                WebClient client = new WebClient();
                //client.Headers["Cache-Control"] = "no-cache";
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadCompletedAtividades);
                client.DownloadStringAsync(new Uri((Application.Current as App).local + "index.php?funcao=pegar_atividades_simples&token=" + (Application.Current as App).token + "&iduser=" + idAluno + "&nocache=" + Environment.TickCount));
            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbCabecalho.Text = "Erro ao atualizar";
                MessageBox.Show("Ops, algo deu errado ao buscar as atividades");
            }

        }
        
        /// <summary>
        /// fazer logout do usuário logado
        /// </summary>
        private void fazerLogout()
        {
            //se nao estiver marcado para ficar logado nao pode deslogar
            if ((Application.Current as App).logado == true)
            {
                if (MessageBox.Show("Deseja realmente desconectar sua conta do Moodle do aplicativo? Será necessário entrar" +
                     " novamente com seu usuário e senha na próxima vez que iniciar o aplicativo, todos os lembretes serão descartados", "Desconectar", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    if (iso.Contains("login"))
                    {
                        iso.Remove("login");
                    }
                    if (iso.Contains("senha"))
                    {
                        iso.Remove("senha");
                    }
                    try
                    {
                        ShellTile TileToFind = ShellTile.ActiveTiles.FirstOrDefault();


                        ShellTileData shellData = new StandardTileData
                        {
                            Title = "UniMoodle",
                            Count = 0,
                            BackContent = "",
                            BackTitle = "",
                            BackBackgroundImage = new Uri("", UriKind.Relative),
                            BackgroundImage = new Uri("icones/300.png", UriKind.Relative)
                        };
                        TileToFind.Update(shellData);

                        //deletar os lembretes ativos
                        foreach (var lemb in lembretes)
                        {
                            ScheduledActionService.Remove(lemb.Name);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    base.NavigationService.RemoveBackEntry();
                    string destino = "/MainPage.xaml";
                    this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
                }
                else
                    return;
            }
            else
                MessageBox.Show("Você não está Conectado! Para se conectar habilite a opção 'Manter Conectado' na próxima vez que iniciar o aplicativo.");
        }

        /// <summary>
        /// atualizar a lista de lembretes
        /// </summary>
        private void atualizaLembretes()
        {
            //se nao estiver marcado para ficar logado nao altera a tile
            if ((Application.Current as App).logado)
            {
                lembretes = ScheduledActionService.GetActions<ScheduledNotification>();
                List<clsLembrete> listaDeLembretes = new List<clsLembrete>();
                if (lstLembretes.Items.Count > 0)
                {
                    lstLembretes.ItemsSource = null;
                }

                foreach (var item in lembretes)
                {
                    if (item.BeginTime < DateTime.Now)
                    {
                        deleta(item.Name);
                    }
                    else
                        listaDeLembretes.Add(new clsLembrete(item.Name, item.BeginTime.ToString(), item.Title));
                }
                lstLembretes.ItemsSource = listaDeLembretes;
                alteraTile(lembretes.Count<ScheduledNotification>(), lstAtividades.Items.Count());
                if (listaDeLembretes.Count > 0)
                {
                    txtbLembretesAtivos.Text = "Lembretes Ativos:";
                }
                else
                    txtbLembretesAtivos.Text = "Sem lembretes";
            }
        }

        /// <summary>
        /// alterar a tile da aplicação
        /// </summary>
        /// <param name="lembretesAtivos"></param>
        /// <param name="tarefas"></param>
        private void alteraTile(int lembretesAtivos, int tarefas)
        {
            //se nao estiver marcado para ficar logado nao altera a tile
            if ((Application.Current as App).logado)
            {
                //cria uma tile
                StandardTileData std = new StandardTileData
                {
                    //número q aparece na tile
                    Count = lembretesAtivos,
                    //texto atrás da tile
                    BackContent = "Existem " + tarefas.ToString() + " tarefas a serem postadas",
                    //Título da aplicação de que aparece atrás da tile
                    BackTitle = lembretesAtivos.ToString() + " lembrete(s) ativo(s)",
                    //Imagem de fundo da parte de trás da tile
                    BackBackgroundImage = new Uri("icones/back.png", UriKind.Relative),
                    //Imagem de fundo da parte da frente da tile
                    BackgroundImage = new Uri("icones/300.png", UriKind.RelativeOrAbsolute),
                    //Título da parte da frente da tile
                    Title = "UniMoodle"
                };
                //atualiza a tile da aplicação
                tile.Update(std);
            }
        }

        /// <summary>
        /// deletar um lembrete
        /// </summary>
        /// <param name="nomeLembrete"></param>
        private void deleta(string nomeLembrete)
        {
            try
            {
                ScheduledActionService.Remove(nomeLembrete);
            }
            catch (Exception)
            {

            }

        }

        #endregion

        #region eventos

        //ao apertar a tecla voltar
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (MessageBox.Show("Sair do aplicativo?", "Sair", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    while (NavigationService.CanGoBack)
                    {
                        NavigationService.RemoveBackEntry();
                    }
                }
                else
                {
                    e.Cancel = true;
                }
                base.OnBackKeyPress(e);
            }
            catch (Exception)
            {
            }

        }

        //quando o retorno do webservice estiver pronto(atividades)
        void client_DownloadCompletedAtividades(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                animaProgressBar(false);
                txtbCabecalho.Text = "Sem resposta do servidor!";
                MessageBox.Show("Ops! Parece que o servidor não está querendo funcionar! Por favor, tente novamente...");
                return;
            }
            else
            {
                try
                {
                    string json = e.Result;
                    Stream stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(json));
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<clsAtividades>));
                    List<clsAtividades> listAtividades;
                    listAtividades = (List<clsAtividades>)serializer.ReadObject(stream);
                    stream.Close();

                    if (listaDeAtividades.Count > 0)
                    {
                        listaDeAtividades.Clear();
                    }
                    if (lstAtividades.Items.Count > 0)
                    {
                        lstAtividades.ItemsSource = null;
                    }
                    foreach (clsAtividades atividade in listAtividades)
                    {
                        listaDeAtividades.Add(new clsAtividades(atividade.id, atividade.itemname, atividade.courseid,
                                                            atividade.date, atividade.nome_curso, atividade.prazo));
                    }
                    lstAtividades.ItemsSource = listaDeAtividades;
                    atualizaLembretes();
                    animaProgressBar(false);
                }
                catch (Exception e2)
                {
                    animaProgressBar(false);
                    //txtbCabecalho.Text = "Erro ao atualizar!";
                    //MessageBox.Show("Ops, algo deu errado ao atualizar as atividades.");
                }
                if (lstAtividades.Items.Count == 0)
                {
                    txtbCabecalho.Text = "Sem tarefas a realizar!";
                }
                else
                    txtbCabecalho.Text = "Próximas tarefas:";
            }
        }

        //quando o retorno do webservice estiver pronto(cursos)
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
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<clsCursos>));
                    List<clsCursos> listCursos;
                    listCursos = (List<clsCursos>)serializer.ReadObject(stream);
                    stream.Close();
                    
                    if (lstCursos.Items.Count > 0)
                    {
                        lstCursos.ItemsSource = null;
                    }
                    if (listaDeCursos.Count > 0)
                    {
                        listaDeCursos.Clear();
                    }
                    foreach (clsCursos curso in listCursos)
                    {
                        listaDeCursos.Add(new clsCursos(curso.id, curso.fullname));
                    }
                    lstCursos.ItemsSource = listaDeCursos;
                    if (listaDeCursos.Count == 0)
                    {
                        txtbVazio.Text = "Sem cursos...";
                    }
                    else
                        txtbVazio.Text = "Meus cursos:";
                    //pegarAtividades(this.id);
                    animaProgressBar(false);
                }
                catch (Exception e2)
                {
                    animaProgressBar(false);
                    txtbVazio.Text = "Erro ao atualizar!";
                    MessageBox.Show("Ops, algo deu errado ao atualizar os cursos.");
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
                (Application.Current as App).idUser = param["id"].ToString();
            }
            if (param.ContainsKey("firstname"))
            {
                txtbNomeAluno.Text = param["firstname"].ToString();
            }
            if (param.ContainsKey("lastname"))
            {
                txtbNomeAluno.Text = txtbNomeAluno.Text + " " + param["lastname"].ToString();
            }
            base.OnNavigatedTo(e);
            atualizaLembretes();
        }

        //quando a página é carregada
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            animaProgressBar(true);
            pegarCursos((Application.Current as App).idUser);
            pegarAtividades((Application.Current as App).idUser);
        }

        //botão fazer logout
        private void Logout_Click(object sender, EventArgs e)
        {
            fazerLogout();
        }

        //botão para atualizar
        private void Atualizar_Click(object sender, EventArgs e)
        {
            animaProgressBar(true);
            pegarCursos((Application.Current as App).idUser);
            pegarAtividades((Application.Current as App).idUser);

        }

        //clique no notão para remover o lembrete
        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (MessageBox.Show("Tem certeza que quer remover este lembrete?", "Remover Lembrete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                deleta((string)((Image)sender).Tag);
                atualizaLembretes();
            }
        }

        //clique do botão sobre
        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Aplicativo UniMoodle v. 1.1.0 - Desenvolvido por: EvaldoRC - 2013");
        }

        //clique do botão avaliar app
        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            MarketplaceReviewTask task = new MarketplaceReviewTask();
            task.Show();
        }

        //clique em alguma atividade
        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string idTarefa = (string)((Grid)sender).Tag;
            string idCurso = "0";
            foreach (clsAtividades atividade in listaDeAtividades)
            {
                if (idTarefa == atividade.id)
                {
                    idCurso = atividade.courseid;
                    break;
                }
            }
            string destino = "/atividadePage.xaml?id=" + idTarefa + "&idCurso=" + idCurso;
            this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
        }

        //clique em algum curso
        private void Grid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string id = (string)((Grid)sender).Tag;
            string nomeCurso = "";
            foreach (clsCursos curso in listaDeCursos)
            {
                if (id == curso.id)
                {
                    nomeCurso = curso.fullname;
                    break;
                }
            }
            string destino = "/cursoPage.xaml?id=" + id + "&nomecurso=" + nomeCurso;
            this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
        }

        #endregion 

        
    }
}