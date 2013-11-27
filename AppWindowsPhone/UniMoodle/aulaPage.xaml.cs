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
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using System.Text;
using System.Text.RegularExpressions;

namespace UniMoodle
{
    public partial class aulaPage : PhoneApplicationPage
    {
        #region atributos

        //nome do arquivo
        string nomeArquivo;
        //download atual
        private BackgroundTransferRequest _currentRequest = null; 
        /// <summary>
        /// id da aula
        /// </summary>
        private string id = null;
        //variável para guardar descricao aula
        private string intro = null;
        //variável para guardar o id do curso
        private string idCurso = null;

        #endregion

        #region construtores

        public aulaPage()
        {
            InitializeComponent();
        }

        #endregion

        #region métodos

        //função para chamar o WebService e pegar os cursos
        private void pegarArquivos(string idCurso, string idAula)
        {
            WebClient client = new WebClient();
            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
            client.DownloadStringAsync(new Uri((Application.Current as App).local + "index.php?funcao=pegar_arquivos&token=" + (Application.Current as App).token + "&idcurso=" + idCurso + "&idaula=" + idAula + "&nocache=" + Environment.TickCount));
            txtbArquivosDisponiveis.Text = "Atualizando...";
            animaProgressBar(true);
        }

        /// <summary>
        /// animar a progressBar
        /// </summary>
        /// <param name="animar">'True' para animar ou 'False' para parar</param>
        private void animaProgressBar(Boolean animar)
        {
            barraProgresso.IsIndeterminate = animar;
            if (animar)
            {
                barraProgresso.Visibility = Visibility.Visible;
            }
            else
            {
                barraProgresso.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// faz download do arquivo
        /// </summary>
        /// <param name="caminho">url do arquivo no servidor</param>
        /// <param name="local">local+nome onde o arquivo vai ser salvo no telefone</param>
        private void downloadArquivo(string caminho, Uri local)
        {
            try
            {
                if (_currentRequest != null)
                {
                    BackgroundTransferService.Remove(_currentRequest);
                }
            }
            catch (Exception)
            {

            }
            DeletaArquivosSeExistem();
            AddTransferRequest(caminho, local);
            InitializeTransferRequestEventHandlers();
            RefreshTransferUI();
        }

        /// <summary>
        /// deleta o arquivo do armazenamento interno
        /// </summary>
        private void DeletaArquivosSeExistem()
        {
            try
            {
                using (IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (userStore.FileExists(nomeArquivo))
                    {
                        userStore.DeleteFile(nomeArquivo);
                    }
                    var arquivos = userStore.GetFileNames("/shared/transfers/*");
                    foreach (var item in arquivos)
                    {
                        userStore.DeleteFile("/shared/transfers/" + item);
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// inicia o download do arquivo
        /// </summary>
        /// <param name="caminho">url do arquivo no servidor</param>
        /// <param name="local">local+nome de onde o arquivo vai ser salvo no telefone</param>
        public void AddTransferRequest(string caminho, Uri local)
        {
            try
            {
                animaProgressBar(true);
                txtbArquivosDisponiveis.Text = "Baixando o arquivo...";
                pbxDownloadAtual.Visibility = Visibility.Visible;
                pbxDownloadAtual.Value = 0;
                imgCancelarDownload.Visibility = Visibility.Visible;

                _currentRequest = new BackgroundTransferRequest(new Uri(caminho), local);
                _currentRequest.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
                BackgroundTransferService.Add(_currentRequest);
            }
            catch (Exception ex)
            {
                
                animaProgressBar(false);
                txtbArquivosDisponiveis.Text = "Arquivos disponíveis para download";
                pbxDownloadAtual.Visibility = Visibility.Collapsed;
                imgCancelarDownload.Visibility = Visibility.Collapsed;
                try
                {
                    BackgroundTransferService.Remove(_currentRequest);
                }
                catch (Exception)
                {
                    MessageBox.Show("Não foi possível iniciar o download");
                }
            }
        }

        /// <summary>
        /// iniciando o download
        /// </summary>
        private void InitializeTransferRequestEventHandlers()
        {
            if (_currentRequest != null)
            {
                _currentRequest.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(_currentRequest_TransferProgressChanged);
                _currentRequest.TransferStatusChanged += new EventHandler<BackgroundTransferEventArgs>(_currentRequest_TransferStatusChanged);
                
            }
        }

        /// <summary>
        /// progresso do download
        /// </summary>
        private void RefreshTransferUI()
        {
            RefreshTransferProgressUI();
            RefreshTransferStatusUI();
        }

        /// <summary>
        /// download completo
        /// </summary>
        private void RefreshTransferStatusUI()
        {
            String statusMessage = "";
            if (_currentRequest != null)
            {
                if (_currentRequest.TransferStatus == TransferStatus.Completed &&
                   _currentRequest.TransferError != null)
                {
                    statusMessage = String.Format("Status: {0}", _currentRequest.TransferError.Message);
                }
                else
                {
                    //animaProgressBar(false);
                    //pbxDownloadAtual.Visibility = Visibility.Collapsed;
                    //imgCancelarDownload.Visibility = Visibility.Collapsed;
                }
            }
            

            //quando download estiver pronto
            if (_currentRequest.TransferStatus == TransferStatus.Completed)
            {
                try
                {
                    animaProgressBar(false);
                    pbxDownloadAtual.Visibility = Visibility.Collapsed;
                    if (imgCancelarDownload.Visibility == Visibility.Collapsed)
                    {
                        txtbArquivosDisponiveis.Text = "Download cancelado pelo usuário";
                        return;
                    }
                    else
                    {
                        imgCancelarDownload.Visibility = Visibility.Collapsed;
                        IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                        if (isf.FileExists(nomeArquivo))
                        {
                            txtbArquivosDisponiveis.Text = "Arquivo foi baixado com sucesso!";
                            DeletaArquivoServidor();
                        }
                        else
                        {
                            txtbArquivosDisponiveis.Text="Não foi possivel encontrar o arquivo, tente novamente!";
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Não foi possivel salvar o arquivo, tente novamente");
                }
            }

        }

        /// <summary>
        /// progresso do download, atualizando a tela
        /// </summary>
        private void RefreshTransferProgressUI()
        {
            String progressMessage = "";
            if (_currentRequest != null)
            {
                double porcentagem = 0;
                porcentagem = _currentRequest.BytesReceived*100;
                porcentagem = porcentagem / _currentRequest.TotalBytesToReceive;
                //progressMessage = String.Format("Baixando...: {0}bytes de {1}bytes ",
                //    _currentRequest.BytesReceived, _currentRequest.TotalBytesToReceive);
                //progressMessage ="Aguarde, baixando..."+ porcentagem.ToString("00")+"%";
                try
                {
                    pbxDownloadAtual.Value = porcentagem;
                }
                catch (Exception)
                {
                    pbxDownloadAtual.Value = 0;
                }
            }
            
        }

        /// <summary>
        /// progresso do download
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _currentRequest_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            RefreshTransferStatusUI();
        }

        /// <summary>
        /// progresso do download...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _currentRequest_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            RefreshTransferProgressUI();
        }

        /// <summary>
        /// chamar o WebService e deletar os arquivos do servidor
        /// </summary>
        private void DeletaArquivoServidor()
        {
            try
            {
                animaProgressBar(true);
                WebClient client2 = new WebClient();
                client2.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadStringCompleted2);
                client2.DownloadStringAsync(new Uri((Application.Current as App).local + "index.php?funcao=deleta_arquivo&token=" + (Application.Current as App).token + "&iduser=" + (Application.Current as App).idUser));
                txtbArquivosDisponiveis.Text = "Processando, aguarde...";
                
            }
            catch (Exception)
            {
                animaProgressBar(false);
                string destino = "/skydrivePage.xaml?modo=upload&arquivo=" + nomeArquivo;
                this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
            }

        }

        #endregion

        #region eventos

        //quando a página é carregada
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            pegarArquivos(idCurso, id);
        }

        //quando o retorno do webservice estiver pronto(arquivos)
        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("Ops! Algo deu errado! Por favor, tente novamente...");
                return;
            }
            else
            {
                try
                {
                    string json = e.Result;
                    Stream stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(json));
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<clsArquivo>));
                    List<clsArquivo> listArquivos;
                    listArquivos = (List<clsArquivo>)serializer.ReadObject(stream);
                    stream.Close();
                    List<clsArquivo> listaDeArquivos = new List<clsArquivo>();
                    if (lstArquivos.Items.Count > 0)
                    {
                        lstArquivos.ItemsSource = null;
                    }
                    foreach (clsArquivo arquivo in listArquivos)
                    {
                        listaDeArquivos.Add(new clsArquivo(arquivo.id, arquivo.filename, arquivo.contenthash));
                    }
                    lstArquivos.ItemsSource = listaDeArquivos;
                    if (listaDeArquivos.Count == 0)
                    {
                        txtbArquivosDisponiveis.Text = "Sem arquivos...";
                    }
                    else
                        txtbArquivosDisponiveis.Text = "Arquivos Disponíveis para Download:";
                }
                catch (Exception e2)
                {
                    txtbArquivosDisponiveis.Text = "Não foi possível buscar os arquivos!";
                }
            }
            animaProgressBar(false);
        }

        //quando o retorno do webservice estiver pronto(deletarArquivo)
        void client_DownloadStringCompleted2(object sender, DownloadStringCompletedEventArgs e)
        {
            animaProgressBar(false);
            if (e.Error != null)
            {
                MessageBox.Show("Ops! Algo deu errado! Por favor, tente novamente...");
                return;
            }
            else
            {
                string resultado = e.Result;
                try
                {
                    string destino = "/skydrivePage.xaml?modo=upload&arquivo=" + nomeArquivo;
                    this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
                }
                catch (Exception e2)
                {
                    MessageBox.Show("Ops! Algo deu errado! Por favor, tente novamente...");
                }
            }
            animaProgressBar(false);
        }

        //ao navegar para esta página
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            foreach (BackgroundTransferRequest request in BackgroundTransferService.Requests)
            {
                _currentRequest = request;
                break;
            }

            IDictionary<string, string> param =
                this.NavigationContext.QueryString;
            if (param.ContainsKey("id"))
            {
                this.id = param["id"].ToString();
            }
            if (param.ContainsKey("intro"))
            {
                this.intro = param["intro"].ToString();
                txtbDescricao.Text = this.intro;
            }
            if (param.ContainsKey("idcurso"))
            {
                this.idCurso = param["idcurso"].ToString();
            }
            base.OnNavigatedTo(e);
        }

        //clicar em algum item
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja baixar este arquivo? Se usado uma conexão móvel, dependendo dos detalhes do seu contrato com o operador de rede móvel, poderão ser aplicadas taxas adicionais.", (string)((Button)sender).Content, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                string id = (string)((Button)sender).Tag;
                string filename = (string)((Button)sender).Content;
                filename = Program.RemoverCaracteresEspeciais(filename);
                nomeArquivo = "/shared/transfers/" + filename;
                Uri saveLocationUri = new Uri(nomeArquivo, UriKind.Relative);
                downloadArquivo((Application.Current as App).local + "index.php?funcao=download&token=" + (Application.Current as App).token +
                    "&iduser=" + (Application.Current as App).idUser + "&idfile=" + id, saveLocationUri);
            }
            else
                return;
        }

        //clique para cancelar o download
        private void imgCancelarDownload_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            imgCancelarDownload.Visibility = Visibility.Collapsed;
            pbxDownloadAtual.Visibility = Visibility.Collapsed;
            if (_currentRequest != null)
            {
                BackgroundTransferService.Remove(_currentRequest);

            }
        }

        #endregion

        
    }
}