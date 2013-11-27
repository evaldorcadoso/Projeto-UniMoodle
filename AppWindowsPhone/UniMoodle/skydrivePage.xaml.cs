using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Live;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Live.Controls;
using System.Text;
using System.IO.IsolatedStorage;
using System.IO;
using System.Threading;
using Microsoft.Phone.BackgroundTransfer;
using System.Net;
using Microsoft.CSharp;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;

namespace UniMoodle
{

    public partial class skydrivePage : PhoneApplicationPage
    {

        #region atributos

        //lista para guardar os items que são mostrados na tela(pastas e ou arquivos)
        List<transactionPastasSkyDrive> ContentList = new List<transactionPastasSkyDrive>();
        public enum StatusDaTela { verpastas, download, upload }
        private StatusDaTela modoTela;
        //
        /// <summary>
        /// controlar se está na pasta inicial, 'True'=não, 'False'=sim
        /// </summary>
        bool aux = false;
        /// <summary>
        /// nome da pasta atual
        /// </summary>
        string pasta="";
        /// <summary>
        /// nome do arquivo para upload 
        /// </summary>
        private string fileName = "";
        /// <summary>
        /// local do aplicativo para salvar o arquivo
        /// </summary>
        private IsolatedStorageFile isf;
        /// <summary>
        /// sessão do skydrive
        /// </summary>
        private LiveConnectClient client;
        //id da tarefa
        string idTarefa;
        //id do curso
        string idCurso;

        #endregion

        #region construtores

        public skydrivePage()
        {
            try
            {
                isf = IsolatedStorageFile.GetUserStoreForApplication();
            }
            catch (Exception)
            {
                txtbStatus.Text= "Opa, isso não era para ter acontecido, tente novamente!";
            }
            InitializeComponent();
        }

        #endregion

        #region métodos
        
        /// <summary>
        /// listar as pastas do usuário conectado
        /// </summary>
        /// <param name="idPasta">id da pasta</param>
        private void listarPastas(string idPasta)
        {
            animaProgressBar(true);
            btnArquivo.Visibility = Visibility.Collapsed;
            try
            {
                txtbStatus.Text = "Buscando Dados...";
                LiveConnectClient cliente = new LiveConnectClient(client.Session);
                cliente.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(cliente_GetCompleted);
                //client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(client_GetCompletedPastas);
                if (idPasta == "")
                {
                    aux = false;
                    cliente.GetAsync("me/skydrive/files?filter=folders");
                    pasta = "me/skydrive";
                }
                else
                {
                    cliente.GetAsync(idPasta + "/files?filter=folders");
                    pasta = idPasta;
                    aux = true;
                }
            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbStatus.Text = "Não conseguimos buscar os dados, tente novamente!";
            }

        }

        /// <summary>
        /// listar tudo, tanto pastas quanto arquivos
        /// </summary>
        /// <param name="idPasta">id da pasta</param>
        private void listarTudo(string idPasta)
        {

            try
            {
                animaProgressBar(true);
                txtbStatus.Text = "Buscando dados...";
                LiveConnectClient clienteTudo = new LiveConnectClient(client.Session);
                clienteTudo.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(clienteTudo_GetCompleted);
                if (idPasta == "")
                {
                    aux = false;
                    clienteTudo.GetAsync("me/skydrive/files");
                }
                else
                {
                    aux = true;
                    clienteTudo.GetAsync(idPasta + "/files");
                }
            }
            catch (Exception ex)
            {
                animaProgressBar(false);
                txtbStatus.Text = "Não conseguimos buscar os dados, tente novamente!";
                //MessageBox.Show(ex.ToString());
            }

        }

        /// <summary>
        /// setar o ícone do arquivo de acordo com sua extensão
        /// </summary>
        /// <param name="fileName">nome do arquivo, já com a extensão</param>
        /// <returns></returns>
        private string SetIconByExtension(string fileName)
        {
            var extension = System.IO.Path.GetExtension(fileName);
            if (extension != null)
            {
                switch (extension.ToUpper())
                {
                    case ".AAC":
                        return "Images/file_aac.png";
                        break;
                    case ".AI":
                        return "Images/file_ai.png";
                        break;
                    case ".AVI":
                        return "Images/file_avi.png";
                        break;
                    case ".BIN":
                        return "Images/file_bin.png";
                        break;
                    case ".BMP":
                        return "Images/file_bmp.png";
                        break;
                    case ".CUE":
                        return "Images/file_cue.png";
                        break;
                    case ".DIVX":
                        return "Images/file_divx.png";
                        break;
                    case ".DOCX":
                        return "Images/file_doc.png";
                        break;
                    case ".DOC":
                        return "Images/file_doc.png";
                        break;
                    case ".EPS":
                        return "Images/file_eps.png";
                        break;
                    case ".FLAC":
                        return "Images/file_flac.png";
                        break;
                    case ".FLV":
                        return "Images/file_flv.png";
                        break;
                    case ".GIF":
                        return "Images/file_gif.png";
                        break;
                    case ".HTML":
                        return "Images/file_html.png";
                        break;
                    case ".ICAL":
                        return "Images/file_ical.png";
                        break;
                    case ".INDD":
                        return "Images/file_indd.png";
                        break;
                    case ".INX":
                        return "Images/file_inx.png";
                        break;
                    case ".ISO":
                        return "Images/file_iso.png";
                        break;
                    case ".JPG":
                        return "Images/file_jpg.png";
                        break;
                    case ".JPEG":
                        return "Images/file_jpg.png";
                        break;
                    case ".MOV":
                        return "Images/file_mov.png";
                        break;
                    case ".MP3":
                        return "Images/file_aac.mp3";
                        break;
                    case ".MPG":
                        return "Images/file_aac.mpeg";
                        break;
                    case ".PDF":
                        return "Images/file_pdf.png";
                        break;
                    case ".PHP":
                        return "Images/file_php.png";
                        break;
                    case ".PNG":
                        return "Images/file_png.png";
                        break;
                    case ".PPS":
                        return "Images/file_pps.png";
                        break;
                    case ".PPT":
                        return "Images/file_ppt.png";
                        break;
                    case ".PSD":
                        return "Images/file_psd.png";
                        break;
                    case ".QXD":
                        return "Images/file_qxd.png";
                        break;
                    case ".QXP":
                        return "Images/file_qxp.png";
                        break;
                    case ".RAW":
                        return "Images/file_raw.png";
                        break;
                    case ".RTF":
                        return "Images/file_rtf.png";
                        break;
                    case ".SVG":
                        return "Images/file_svg.png";
                        break;
                    case ".TIF":
                        return "Images/file_tif.png";
                        break;
                    case ".TXT":
                        return "Images/file_txt.png";
                        break;
                    case ".VCF":
                        return "Images/file_vcf.png";
                        break;
                    case ".WAV":
                        return "Images/file_wav.png";
                        break;
                    case ".WMA":
                        return "Images/file_wma.png";
                        break;
                    case ".XLSX":
                        return "Images/file_xls.png";
                        break;
                    case ".XLS":
                        return "Images/file_xls.png";
                        break;
                    case ".WMV":
                        return "Images/jewel_case.png";
                        break;
                    case ".ZIP":
                        return "Images/box_zip.png";
                        break;
                    case ".RAR":
                        return "Images/box_rar.png";
                        break;
                    default:
                        return "Images/universal_binary.png";
                        break;

                }
            }
            else
            {
                return "Images/funiversal_binary.png";
            }
        }

        /// <summary>
        /// animar a progressBar
        /// </summary>
        /// <param name="animar">'True' para animar ou 'False' para parar</param>
        private void animaProgressBar(Boolean animar)
        {
            try
            {
                barraProgresso.IsIndeterminate = animar;
                if (animar)
                    barraProgresso.Visibility = Visibility.Visible;
                else
                    barraProgresso.Visibility = Visibility.Collapsed;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// fazer o upload do arquivo que está no armazenamento interno do telefone
        /// </summary>
        private void fazerUpload()
        {
            try
            {
                animaProgressBar(true);
                btnArquivo.Visibility = Visibility.Collapsed;
                txtbStatus.Text = "Subindo arquivo, aguarde...";
                client.UploadCompleted += new EventHandler<LiveOperationCompletedEventArgs>(client_UploadCompleted);
                client.UploadProgressChanged += new EventHandler<LiveUploadProgressChangedEventArgs>(client_UploadProgressChanged);
                using (isf)
                {
                    IsolatedStorageFileStream stream = isf.OpenFile(fileName, FileMode.Open);
                    client.UploadAsync(pasta, fileName.Substring(18), stream, OverwriteOption.Overwrite);
                }
            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbStatus.Text = "Não conseguimos subir o arquivo, tente novamente!";
                btnArquivo.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// pergunta se deseja fazer o download, e o inicia
        /// </summary>
        /// <param name="id">id do arquivo</param>
        private void chamaDownload(string id)
        {
            client.AttachPendingTransfers();
            foreach (transactionPastasSkyDrive file in ContentList)
            {
                if (id == file.id)
                {
                    //fazer download
                    HyperlinkButton hyperlinkButton = new HyperlinkButton()
                    {
                        Content = file.name,
                        Margin = new Thickness(0, 28, 0, 8),
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };
                    CustomMessageBox mensagem = new CustomMessageBox();
                    mensagem.Caption = "Baixar arquivo?";
                    mensagem.Message = "Você quer escolher este arquivo para postar no Moodle? Se usado uma conexão móvel, dependendo dos detalhes do seu contrato com o operador de rede móvel, poderão ser aplicadas taxas adicionais.";
                    mensagem.Content = hyperlinkButton;
                    mensagem.LeftButtonContent = "Sim";
                    mensagem.RightButtonContent = "Não";
                    mensagem.Dismissed += (s1, e1) =>
                    {
                        switch (e1.Result)
                        {
                            case CustomMessageBoxResult.LeftButton:
                                animaProgressBar(true);
                                //limpa o armazenamento interno do telefone
                                DeletaArquivosSeExistem();
                                file.name = Program.RemoverCaracteresEspeciais(file.name);
                                file.name = Program.TiraAcentos(file.name);
                                fileName = file.name;
                                txtbStatus.Text = "Fazendo Download - 0%";
                                client.DownloadProgressChanged += new EventHandler<LiveDownloadProgressChangedEventArgs>(client_BackgroundDownloadProgressChanged);
                                //client.DownloadCompleted += new EventHandler<LiveOperationCompletedEventArgs>(client_BackgroundDownloadCompleted);
                                client.DownloadCompleted += new EventHandler<LiveDownloadCompletedEventArgs>(client_DownloadCompleted);
                                client.DownloadAsync(id + "/content");
                                break;
                            case CustomMessageBoxResult.RightButton:
                                //do anything
                                break;
                            default:
                                break;
                        }
                    };
                    mensagem.Show();
                    break;
                }
                else
                {
                    txtbStatus.Text = "Opa, isso não era para ter acontecido, tente novamente!";
                }
            }
        }

        /// <summary>
        /// deletar todos os arquivos do armaznamento interno
        /// </summary>
        private void DeletaArquivosSeExistem()
        {
            try
            {
                using (IsolatedStorageFile userStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
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

        #endregion

        #region eventos

        //ao navegar para esta página
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            IDictionary<string, string> param = this.NavigationContext.QueryString;
            try
            {
                if (param.ContainsKey("modo"))
                {
                    string modo = param["modo"].ToString();
                    switch (modo)
                    {
                        case "verpastas":
                            {
                                modoTela = StatusDaTela.verpastas;
                                break;
                            }
                        case "download":
                            {
                                modoTela = StatusDaTela.download;
                                break;
                            }
                        case "upload":
                            {
                                modoTela = StatusDaTela.upload;
                                break;
                            }
                        default:
                            break;
                    }
                }
                if (param.ContainsKey("arquivo"))
                {
                    this.fileName = param["arquivo"].ToString();
                    btnArquivo.Content = "Subir " + fileName.Substring(18) + " aqui";
                }
                if (param.ContainsKey("id"))
                {
                    idTarefa = param["id"].ToString();
                }
                if (param.ContainsKey("idCurso"))
                {
                    this.idCurso = param["idCurso"].ToString();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Ops, algo deu errado, tente novamente!");
            }
            base.OnNavigatedTo(e);
        }

        //dowload em progresso
        void client_BackgroundDownloadProgressChanged(object sender, LiveDownloadProgressChangedEventArgs e)
        {
            txtbStatus.Text = "Baixando o arquivo, aguarde... " + e.ProgressPercentage.ToString()+" %";
        }

        //clique do botão upload
        private void Upload_Click(object sender, EventArgs e)
        {
            listarPastas("");
        }

        //retorno dos dados do usuário
        void client_GetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            try
            {
                txtbStatus.Text = "";
                string usuarioConectado = "";
                if (e.Error == null)
                {
                    if (e.Result.ContainsKey("name"))
                    {
                        usuarioConectado = e.Result["name"].ToString();
                    }
                    else
                    {
                        if (e.Result.ContainsKey("firstname"))
                        {
                            usuarioConectado = e.Result["firstname"].ToString();
                        }
                    }
                }
                txtbConectado.Text = "Conectado como: " + usuarioConectado;
                txtbTitulodapágina.Text = "skydrive de " + usuarioConectado;
                txtbTitulodapágina.Text = txtbTitulodapágina.Text.ToLower();
            }
            catch (Exception ex)
            {
                animaProgressBar(false);
                txtbStatus.Text = "Opa, não conseguimos encontrar seu nome de usuário!";
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                animaProgressBar(false);
                if (modoTela == StatusDaTela.upload)
                {
                    listarPastas("");
                }
                if (modoTela == StatusDaTela.download)
                {
                    listarTudo("");
                }
            }
        }

        //retorno de listar as pastas
        void cliente_GetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    ContentList.Clear();
                    lstFolders.ItemsSource = null;
                    List<object> data = (List<object>)e.Result["data"];

                    foreach (IDictionary<string, object> content in data)
                    {
                        transactionPastasSkyDrive skyContent = new transactionPastasSkyDrive();
                        skyContent.id = (string)content["id"];
                        skyContent.name = (string)content["name"];
                        skyContent.Size = (string)content["size"].ToString();
                        if ((string)content["type"] == "folder")
                        {
                            skyContent.tipo = "icones/appbar.folder.png";
                        }
                        else
                            skyContent.tipo = "icones/appbar.page.png";
                        ContentList.Add(skyContent);
                    }
                    lstFolders.ItemsSource = ContentList;
                    txtbStatus.Text = "";
                    btnArquivo.Visibility = Visibility.Visible;
                    animaProgressBar(false);
                }
            }
            catch (Exception ex)
            {
                animaProgressBar(false);
                //MessageBox.Show(ex.ToString());
                txtbStatus.Text = "Não conseguimos obter os dados, tente novamente";
            }
        }

        //retorno do listar tudo completo
        void clienteTudo_GetCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    ContentList.Clear();
                    lstFolders.ItemsSource = null;
                    List<object> data = (List<object>)e.Result["data"];

                    foreach (IDictionary<string, object> content in data)
                    {
                        transactionPastasSkyDrive skyContent = new transactionPastasSkyDrive();
                        skyContent.id = (string)content["id"];
                        skyContent.name = (string)content["name"];
                        skyContent.Size = (string)content["size"].ToString();
                        if ((string)content["type"] == "folder")
                        {
                            skyContent.tipo = "icones/appbar.folder.png";
                        }
                        else
                            skyContent.tipo = SetIconByExtension(skyContent.name);
                        ContentList.Add(skyContent);
                    }
                    lstFolders.ItemsSource = ContentList;
                    lstFolders.SelectedIndex = 0;
                    txtbStatus.Text = "";
                    animaProgressBar(false);
                }
                else
                {
                    animaProgressBar(false);
                    txtbStatus.Text = "Opa, isso não era para ter acontecido, tente novamente!";
                }
            }
            catch (Exception ex)
            {
                animaProgressBar(false);
                txtbStatus.Text = "Não conseguimos obter os dados, tente novamente!";
                //MessageBox.Show(ex.ToString());
            }
        }

        //evento da sessão do SkyDrive
        private void signInButton1_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            animaProgressBar(true);
            try
            {
                if (e.Status == LiveConnectSessionStatus.Connected)
                {
                    txtbStatus.Text = "Atualizando informações da conta...";
                    txtbConectado.Text = "Conectado como:";
                    txtbInfo.Visibility = Visibility.Collapsed;
                    client = new LiveConnectClient(e.Session);
                    client.AttachPendingTransfers();
                    client.GetCompleted += new EventHandler<LiveOperationCompletedEventArgs>(client_GetCompleted);
                    client.GetAsync("me");
                }
                else
                {
                    ContentList.Clear();
                    lstFolders.ItemsSource = null;
                    txtbConectado.Text = "Desconectado";
                    txtbInfo.Visibility = Visibility.Visible;
                    txtbStatus.Text = "";
                    txtbTitulodapágina.Text = "skydrive";
                    animaProgressBar(false);
                }
            }
            catch (Exception)
            {
                animaProgressBar(false);
                MessageBox.Show("Não conseguimos verificar a conexão com o SkyDrive, tente novamente!");
            }
            
        }

        //upload em progresso
        void client_UploadProgressChanged(object sender, LiveUploadProgressChangedEventArgs e)
        {
            txtbStatus.Text = "Subindo arquivo, aguarde... " + e.ProgressPercentage.ToString() + "%";
        }

        //upload completo
        void client_UploadCompleted(object sender, LiveOperationCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    animaProgressBar(false);
                    MessageBox.Show("Arquivo foi carregado com sucesso!");
                    btnArquivo.Visibility = Visibility.Collapsed;
                    btnArquivo.Content = "";
                    txtbStatus.Text = "";
                    NavigationService.RemoveBackEntry();
                    this.NavigationService.GoBack();
                }
                else
                {
                    animaProgressBar(false);
                    txtbStatus.Text = "Opa, isso não era para ter acontecido, tente novamente!";
                }
            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbStatus.Text = "Não conseguimos subir o arquivo, tente novamente!";
            }
        }

        //clique em algum item
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tipo = (string)((Button)sender).CommandParameter;
                string id = (string)((Button)sender).Tag;
                //se for pasta
                if (tipo == "icones/appbar.folder.png")
                {
                    if (modoTela == StatusDaTela.upload)
                    {
                        listarPastas(id);
                    }
                    else
                    {
                        if (modoTela == StatusDaTela.download)
                        {
                            listarTudo(id);
                        }
                    }
                }
                else
                {
                    //se for arquivo, encontra o arquivo
                    chamaDownload(id); 
                }
            }
            catch (Exception ex)
            {
                txtbStatus.Text = "Parece que este arquivo não quer ser incomodado no momento, tente novamente!";
            } 
        }

        //download completo
        void client_DownloadCompleted(object sender, LiveDownloadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                try
                {
                    Stream stream = e.Result;
                    using (isf)
                    {
                        using (IsolatedStorageFileStream filetosave = isf.OpenFile("/shared/transfers/" + fileName, FileMode.Create, FileAccess.ReadWrite))
                        {
                            stream.CopyTo(filetosave);
                            stream.Flush();
                            if (isf.FileExists("/shared/transfers/" + fileName))
                            {
                                stream.Close();
                                animaProgressBar(false);
                                txtbStatus.Text = "Download completo.";
                                base.NavigationService.RemoveBackEntry();
                                //volta para a página de atividade
                                string destino = "/atividadePage.xaml?nomeArquivo=" + fileName + "&id=" + idTarefa + "&idCurso=" + idCurso;
                                this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
                            }
                            else
                            {
                                animaProgressBar(false);
                                txtbStatus.Text = "Não achamos o arquivo, tente novamente!";
                                //MessageBox.Show("Arquivo não encontrado no armazenamento interno.");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    animaProgressBar(false);
                    MessageBox.Show("Não conseguimos realizar o download, tente novamente!");
                }
            }
            else
            {
                animaProgressBar(false);
                txtbStatus.Text = "Opa, isso não era para ter acontecido, tente novamente!";
            }
            
        }

        //ao apertar a tecla voltar
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                client.AttachPendingTransfers();
                if (aux)
                {
                    if (modoTela == StatusDaTela.download)
                    {
                        listarTudo("");
                    }
                    else
                    {
                        listarPastas("");
                    }
                    e.Cancel = true;
                }
                else
                {
                    if (modoTela == StatusDaTela.download)
                    {
                        if (MessageBox.Show("Deseja sair desta tela sem baixar o arquivo no SkyDrive?", "Sair", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            base.OnBackKeyPress(e);     
                        }
                        else
                            e.Cancel = true;
                    }
                    if (modoTela == StatusDaTela.upload)
                    {
                        if (MessageBox.Show("Deseja fechar esta tela sem salvar o arquivo no SkyDrive?", "Sair", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            base.OnBackKeyPress(e);
                        }
                        else
                            e.Cancel = true;
                    }
                    base.OnBackKeyPress(e);
                }
            }
            catch (Exception)
            {
                txtbStatus.Text = "Opa, isso não era para ter acontecido, tente novamente!";
            }

        }

        //clique no botão do arquivo para upload
        private void btnArquivo_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja subir este arquivo: " + this.fileName.Substring(18) + " ? Se usado uma conexão móvel, dependendo dos detalhes do seu contrato com o operador de rede móvel, poderão ser aplicadas taxas adicionais.", "Subir arquivo", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
            //faz upload
            fazerUpload();
            }
        }

        #endregion
    }
}