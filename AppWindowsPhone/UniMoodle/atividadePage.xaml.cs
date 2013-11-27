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
using RestSharp;
using System.IO.IsolatedStorage;
using System.IO;
using System.Runtime.Serialization.Json;
using Microsoft.Phone.Scheduler;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace UniMoodle
{
    public partial class atividadePage : PhoneApplicationPage
    {
        #region atributos

        IEnumerable<ScheduledNotification> lembrete;
        //tarefa atual
        clsTarefa tarefaAtual = new clsTarefa();
        /// <summary>
        /// id do curso atual
        /// </summary>
        string idCurso="";

        #endregion

        #region construtores

        public atividadePage()
        {
            InitializeComponent();
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-br");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-br");
        }

        #endregion

        #region métodos

        /// <summary>
        /// busca o lembrete salvo e atualiza a tela
        /// </summary>
        private void pegaLembrete()
        {
            if (beginTimePicker.Visibility == Visibility.Collapsed)
            {
                lembrete = ScheduledActionService.GetActions<ScheduledNotification>();
                var myReminder = ScheduledActionService.GetActions<ScheduledNotification>().Where(a => a.Name == tarefaAtual.id).FirstOrDefault();
                
                if (myReminder != null)
                {
                    imgBotaoLembrete.Source = new BitmapImage(new Uri("icones/appbar.delete.png", UriKind.Relative));
                    gridBotaoLembrete.Background = new SolidColorBrush(Colors.Red);
                    beginDatePicker.Value = myReminder.BeginTime.Date;
                    beginTimePicker.Value = myReminder.BeginTime;
                    beginDatePicker.Visibility = Visibility.Visible;
                    beginTimePicker.Visibility = Visibility.Visible;
                    txtbDataLembrete.Visibility = Visibility.Collapsed;
                    imgBotaoLembrete.Tag = "1";
                }
                else
                    return;
            }
            else
                return;
        }

        /// <summary>
        /// converter bytes para megabytes
        /// </summary>
        /// <param name="bytes">tamanho em bytes do arquivo</param>
        /// <returns>tamanho em megabytes do arquivo</returns>
        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        /// <summary>
        /// converter bytes para kbytes
        /// </summary>
        /// <param name="bytes">tamanho em bytes do arquivo</param>
        /// <returns>tamanho em megabytes do arquivo</returns>
        static double ConvertBytesToKbytes(long bytes)
        {
            return (bytes / 1024f);
        }

        /// <summary>
        /// faz upload do arquivo para o Moodle e posta a tarefa
        /// </summary>
        private void fazUploadProServidor()
        {
            try
            {
                RestRequest request = new RestRequest((Application.Current as App).local + "index.php?token=" + (Application.Current as App).token + "&funcao=upload&idtarefa=" + tarefaAtual.id + "&iduser=" + (Application.Current as App).idUser + "&idcurso=" + idCurso + "&maxbytes=" + tarefaAtual.maxbytes, Method.POST);
                string path = "/shared/transfers/" + txtbNomeArquivo.Text;
                IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication();
                if (isf.FileExists(path))
                {
                    //MessageBox.Show("arquivo encontrado");
                    IsolatedStorageFileStream sourcestream = isf.OpenFile(path, FileMode.OpenOrCreate);
                    FileStream fs = sourcestream;
                    long tamanhoArquivo = fs.Length;
                    //ver se o tamanho do arquivo pode ser postado
                    if (tamanhoArquivo > Convert.ToInt64(tarefaAtual.maxbytes))
                    {
                        MessageBox.Show("O arquivo que você escoheu excede o limite máximo definido para esta postagem! Por favor escolha outro arquivo.");
                        animaProgressBar(false);
                        txtbDescricao.Text = "Descrição da Tarefa:";
                    }
                    else
                    {
                        
                        request.AddFile("file", ReadToEnd(fs), txtbNomeArquivo.Text, "file");
                        RestClient restClient = new RestClient();

                        restClient.ExecuteAsync(request, (response) =>
                        {
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                //upload successfull
                                MessageBox.Show("Postagem realizada com sucesso!!\n" + response.Content);
                                //deleta o lembrete
                                deleta(tarefaAtual.id);
                                if (isf.FileExists(path))
                                {
                                    try
                                    {
                                        isf.DeleteFile(path);
                                    }
                                    catch (Exception)
                                    {

                                    }
                                }
                                animaProgressBar(false);
                                txtbDescricao.Text = "Descrição da Tarefa";
                                //this.NavigationService.RemoveBackEntry();
                                NavigationService.GoBack();
                                //volta para a página de menu
                            }
                            else
                            {
                                animaProgressBar(false);
                                txtbDescricao.Text = "Descrição da Tarefa";
                                //ocorreu um erro durante o upload
                                //MessageBox.Show(response.StatusCode + "\n" + response.StatusDescription);
                                MessageBox.Show("Desculpe, não foi possível postar o arquivo");
                            }
                        });
                    }
                }

                else
                {
                    animaProgressBar(false);
                    txtbDescricao.Text = "Descrição da Tarefa";
                    txtbNomeArquivo.Text = "Nenhum arquivo Selecionado";
                    MessageBox.Show("Selecione o arquivo novamente!");
                }

            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbDescricao.Text = "Descrição da Tarefa";
            }
        }

        /// <summary>
        /// lê o tamanho do arquivo em stream e retorna em bytes
        /// </summary>
        /// <param name="stream">arquivo</param>
        /// <returns>bytes do arquivo</returns>
        public byte[] ReadToEnd(System.IO.Stream stream)
        {
            long originalPosition = stream.Position;
            stream.Position = 0;
            long info = stream.Length;
            //string s = ConvertBytesToMegabytes(info).ToString("0.00");
            //MessageBox.Show("Tamanho deste arquivo: " + info.ToString() + " bytes");
            try
            {
                //byte[] readBuffer = new byte[4096];
                byte[] readBuffer = new byte[5242880];


                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                stream.Position = originalPosition;
            }
        }

        /// <summary>
        /// chamar o WebService e pegar os dados da atividade
        /// </summary>
        private void pegarAtividades()
        {
            try
            {
                txtbDescricao.Text = "Atualizando...";
                animaProgressBar(true);
                WebClient client = new WebClient();
                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(client_DownloadCompletedAtividades);
                client.DownloadStringAsync(new Uri((Application.Current as App).local + "index.php?funcao=pegar_atividades_detalhada&idcurso=" + idCurso + "&idtarefa=" + tarefaAtual.id + "&token=" + (Application.Current as App).token + "&nocache=" + Environment.TickCount));
            }
            catch (Exception)
            {
                animaProgressBar(false);
                txtbDescricao.Text = "Erro ao atualizar";
                //MessageBox.Show("Ops, algo deu errado ao buscar a atividade");
            }

        }

        /// <summary>
        /// ativar o lembrete da tarefa
        /// </summary>
        private void ativar()
        {
            //se nao estiver marcado para ficar logado nao cria lembrete
            if ((Application.Current as App).logado)
            {
                // The code in the following steps goes here.
                //String name = System.Guid.NewGuid().ToString();
                string name = tarefaAtual.id.ToString();

                RecurrenceInterval recurrence = RecurrenceInterval.None;

                DateTime date = (DateTime)beginDatePicker.Value;
                DateTime time = (DateTime)beginTimePicker.Value;
                DateTime beginTime = date + time.TimeOfDay;
                //o tempo não pode ser inferior ao atual
                if (beginTime < DateTime.Now)
                {
                    MessageBox.Show("A hora e/ou o dia selecionado é inferior a data/hora atual");
                    return;
                }

                DateTime expirationTime = date + time.TimeOfDay;

                string destino = "/atividadePage.xaml?id=" + tarefaAtual.id + "&idCurso=" + idCurso + "&idAluno=" + (Application.Current as App).idUser;

                Reminder lembrete = new Reminder(name);
                lembrete.Title = tarefaAtual.fullname;
                lembrete.Content = "Você tem uma tarefa para ser postada no Moodle, toque aqui para postá-la agora mesmo...";
                lembrete.BeginTime = beginTime;

                lembrete.ExpirationTime = expirationTime;
                lembrete.RecurrenceType = recurrence;
                lembrete.NavigationUri = new Uri(destino, UriKind.Relative);
                try
                {
                    if (ScheduledActionService.Find(tarefaAtual.id.ToString()) != null)
                    {
                        deleta(tarefaAtual.id.ToString());
                        ScheduledActionService.Add(lembrete);
                    }
                    else
                    {
                        ScheduledActionService.Add(lembrete);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Erro ao salvar o lembrete");
                }
            }
        }

        /// <summary>
        /// excluir o lembrete
        /// </summary>
        /// <param name="nomeLembrete">nome do lembrete</param>
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
                txtbDescricao.Text = "Próximas Atividades";
                barraProgresso.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region eventos

        //ao navegar para esta página
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            IDictionary<string, string> param =
                this.NavigationContext.QueryString;
            if (param.ContainsKey("nomeArquivo"))
            {
                base.NavigationService.RemoveBackEntry();
                txtbNomeArquivo.Text = param["nomeArquivo"].ToString();
                btnEnviarArquivo.IsEnabled = true;
            }
            if (param.ContainsKey("id"))
            {
                tarefaAtual.id = param["id"].ToString();
            }
            if (param.ContainsKey("idCurso"))
            {
                this.idCurso = param["idCurso"].ToString();
            }
            if (param.ContainsKey("idAluno"))
            {
                (Application.Current as App).idUser = param["idAluno"].ToString();
            }
            pegarAtividades();
            pegaLembrete();
            base.OnNavigatedTo(e);
        }

        //ao sair desta página
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                //salvar ou nao o lembrete
                if (imgBotaoLembrete.Tag.ToString() == "1")
                {
                    ativar();
                }
                else
                {
                    deleta(tarefaAtual.id.ToString());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Ops, não foi possível verificar o lembrete para esta tarefa, tente novamente!");
            }
        }
        
        //pegar arquivo do skydrive
        private void btnSelecionarArquivo_Click(object sender, RoutedEventArgs e)
        {
            string destino = "/skydrivePage.xaml?modo=download&id="+tarefaAtual.id+"&idCurso="+idCurso;
            this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
        }

        //clique no botão para enviar o arquivo
        private void btnSelecionarArquivo_Copy_Click(object sender, RoutedEventArgs e)
        {
            animaProgressBar(true);
            txtbDescricao.Text = "Enviando, aguarde...";
            btnEnviarArquivo.IsEnabled = false;
            fazUploadProServidor();
        }

        //quando o retorno do webservice estiver pronto(atividade)
        void client_DownloadCompletedAtividades(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                animaProgressBar(false);
                txtbDescricao.Text = "Sem resposta do servidor!";
                txtbDescricaoAtividade.Text="Ops! Parece que o servidor não está querendo funcionar, Tente novamente!";
                return;
            }
            else
            {
                try
                {
                    string json = e.Result;
                    Stream stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(json));
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<clsTarefa>));
                    List<clsTarefa> listTarefas;
                    listTarefas = (List<clsTarefa>)serializer.ReadObject(stream);
                    stream.Close();
                    List<clsTarefa> listaDeTarefas = new List<clsTarefa>();
                    
                    foreach (clsTarefa tarefa in listTarefas)
                    {
                        tarefaAtual.id = tarefa.id;
                        tarefaAtual.intro = tarefa.intro;
                        tarefaAtual.duedate = tarefa.duedate;
                        tarefaAtual.fullname = tarefa.fullname;
                        tarefaAtual.maxbytes = tarefa.maxbytes;
                        
                        break;
                    }
                    string strSemHtml = System.Text.RegularExpressions.Regex.Replace(tarefaAtual.intro, "<[^>]*>", " ");
                    txtbDescricaoAtividade.Text = strSemHtml;
                    txtbDataEntrega.Text = tarefaAtual.duedate;
                    txtbCurso.Text = tarefaAtual.fullname;
                    txtbDescricao.Text = "Descrição da Tarefa:";
                    txtbMaxBytes.Text = "Limite de tamanho: " + ConvertBytesToMegabytes(Convert.ToInt64(tarefaAtual.maxbytes)) + "Mb";
                    animaProgressBar(false);
                }
                catch (Exception e2)
                {
                    animaProgressBar(false);
                    txtbDescricao.Text = "Erro ao atualizar!";
                    txtbDescricaoAtividade.Text="Ops, algo deu errado ao buscar os dados, tente novamente!";
                }
            }
        }

        //clique para adicionar/remover lembrete
        private void imgBotaoLembrete_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //se noa estiver marcado para ficar logado nao altera a tile
            if ((Application.Current as App).logado)
            {
                try
                {
                    if (imgBotaoLembrete.Tag.ToString() == "0")
                    {
                        string sdata = txtbDataEntrega.Text;
                        string shora = txtbDataEntrega.Text;
                        beginDatePicker.Value = Convert.ToDateTime(sdata.Substring(0, 10));
                        beginTimePicker.Value = Convert.ToDateTime(shora.Substring(11)).AddMinutes(-30);
                        gridBotaoLembrete.Background = new SolidColorBrush(Colors.Red);
                        imgBotaoLembrete.Source = new BitmapImage(new Uri("icones/appbar.delete.png", UriKind.Relative));
                        beginDatePicker.Visibility = Visibility.Visible;
                        beginTimePicker.Visibility = Visibility.Visible;
                        txtbDataLembrete.Visibility = Visibility.Collapsed;
                        imgBotaoLembrete.Tag = "1";
                    }
                    else
                    {
                        imgBotaoLembrete.Source = new BitmapImage(new Uri("icones/appbar.add.png", UriKind.Relative));
                        gridBotaoLembrete.Background = new SolidColorBrush(Colors.Transparent);
                        beginDatePicker.Visibility = Visibility.Collapsed;
                        beginTimePicker.Visibility = Visibility.Collapsed;
                        txtbDataLembrete.Visibility = Visibility.Visible;
                        imgBotaoLembrete.Tag = "0";
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Opa, isso não era para ter acontecido, tente novamente!");
                }
            }
            else
                MessageBox.Show("Você não pode criar lembretes se não estiver logado permanentemente, para resolver isto, tente logar novamente e marque a opção 'Manter Logado' na tela de Login");
        }

        #endregion
    }
}
