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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Phone.Tasks;


namespace UniMoodle
{
    public partial class MainPage : PhoneApplicationPage
    {
        #region atributos

        /// <summary>
        /// armazenamento local do aplicativo
        /// </summary>
        IsolatedStorageSettings iso = IsolatedStorageSettings.ApplicationSettings;

        #endregion

        #region construtores
        
        public MainPage()
        {
            InitializeComponent();
            if ((iso.Contains("login")) && (iso.Contains("senha")))
            {
                
                animaProgressBar(true);
                chamaWS(iso["login"].ToString(), iso["senha"].ToString());
                txtbLogin.Text = iso["login"].ToString();
                psbSenha.Password = iso["senha"].ToString();
                cbxManterLogado.IsChecked = true;
            }
        }

        #endregion

        #region métodos

        /// <summary>
        /// chama o WebSeervice para fazer o login
        /// </summary>
        /// <param name="login"></param>
        /// <param name="senha"></param>
        private void chamaWS(string login, string senha)
        {
            try
            {
                WebClient wc = new WebClient();
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                string myParameters = String.Format("passwor=" + senha);
                Uri URI = new Uri((Application.Current as App).local + "/index.php?funcao=login&token=" + (Application.Current as App).token + "&login=" + login + "&nocache=" + Environment.TickCount);

                wc.UploadStringAsync(URI, myParameters);
                wc.UploadStringCompleted += new System.Net.UploadStringCompletedEventHandler(wc_UploadStringCompleted);

            }
            catch (Exception e)
            {
                animaProgressBar(false);
                MessageBox.Show("erro chamaWS: " + e.ToString());
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
                btnEntrar.Content = "Entrando";
                barraProgresso.Visibility = Visibility.Visible;
            }
            else
            {
                btnEntrar.Content = "Entrar";
                barraProgresso.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        #region eventos

        //botão para fazer login
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if ((txtbLogin.Text == "usuário") && (psbSenha.Password == "senha"))
                {
                    MessageBox.Show("Entre com seu usuário e senha de sua conta Moodle");
                }
                else
                {
                    animaProgressBar(true);
                    if ((bool)cbxManterLogado.IsChecked)
                    {
                        (Application.Current as App).logado = true;
                        //É necessário perguntar se a chave já existe para evitar que se crie chaves duplicadas, o que fatalmente irá                       provocar erros
                        if ((iso.Contains("login")) && (iso.Contains("senha"))) //Apenas atualiza os valores das chaves
                        {
                            iso["login"] = txtbLogin.Text;
                            iso["senha"] = psbSenha.Password;
                        }
                        else //Cria novas chaves
                        {
                            iso.Add("login", txtbLogin.Text);
                            iso.Add("senha", psbSenha.Password);
                        }
                        iso.Save(); //Necessário para armazenar os valores das chaves
                    }
                    else
                    {
                        (Application.Current as App).logado = false;
                        //Caso o usuário não queira mais armazenar suas credenciais, podemos remover as chaves
                        if (iso.Contains("login"))
                        {
                            iso.Remove("login");
                        }
                        if (iso.Contains("senha"))
                        {
                            iso.Remove("senha");
                        }
                    }
                    chamaWS(txtbLogin.Text, psbSenha.Password);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Ops, algo deu errado. Por favor tente novamente...");
            }
        }

        //quando o retorno do webservice estiver pronto(login)
        void wc_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            //MessageBox.Show(e.Result); 
            //e.result fetches you the response against your POST request.         
            if (e.Error != null)
            {
                animaProgressBar(false);
                MessageBox.Show("Ops! Parece que o servidor não está querendo funcionar! Por favor, tente novamente...");
                return;
            }
            else
            {
                try
                {
                    //se estiver logado 
                    if ((bool)cbxManterLogado.IsChecked)
                        (Application.Current as App).logado = true;
                    else
                        (Application.Current as App).logado = false;
                    string json = e.Result;
                    Stream stream = new MemoryStream(System.Text.Encoding.Unicode.GetBytes(json));
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<clsAluno>));
                    List<clsAluno> listAluno;
                    listAluno = (List<clsAluno>)serializer.ReadObject(stream);
                    stream.Close();
                    animaProgressBar(false);
                    if (listAluno[0].id != "")
                    {

                        //chama a página de menu
                        string destino = "/menuPage.xaml?id=" + listAluno[0].id + "&firstname=" + listAluno[0].firstname + "&lastname=" + listAluno[0].lastname;
                        this.NavigationService.Navigate(new Uri(destino, UriKind.Relative));
                    }
                    else
                    {
                        animaProgressBar(false);
                        MessageBox.Show("Não foi possível fazer login");
                    }
                }
                catch (Exception e2)
                {
                    animaProgressBar(false);
                    MessageBox.Show("Ops, parece que há algo errado com seu login e/ou senha.");
                }

            }
        }

        //clique no botão para fazer logout
        private void Logout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Ao desconetar sua conta, seus dados não serão mais sincronizados.\nTem certeza que deseja desconectar sua conta do Moodle?", "Desconectar", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //remove as definições de login e senha armazenadas no dispositivo
                if (iso.Contains("login"))
                {
                    iso.Remove("login");
                }
                if (iso.Contains("senha"))
                {
                    iso.Remove("senha");
                }
                //retorna a tela de login
                while (NavigationService.CanGoBack)
                {
                    NavigationService.RemoveBackEntry();
                }
            }
        }

        //ao apertar a tecla voltar
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Sair do aplicativo?", "Sair", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //Encerra o aplicativo
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

        //clicar na textboxLogin
        private void txtbLogin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (txtbLogin.Text == "usuário")
                txtbLogin.Text = "";
        }

        //clicar na textboxSenha
        private void psbSenha_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (psbSenha.Password == "senha")
                psbSenha.Password = "";
        }

        //deixar a textboxLogin
        private void txtbLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtbLogin.Text == "")
                txtbLogin.Text = "usuário";
        }

        //deixar a textboxSenha
        private void psbSenha_LostFocus(object sender, RoutedEventArgs e)
        {
            if (psbSenha.Password == "")
                psbSenha.Password = "senha";
        }

        //clique em cadastrar
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton hyperlinkButton = new HyperlinkButton()
            {
                Content = "Criar conta agora",
                Margin = new Thickness(0, 28, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Left,
                NavigateUri = new Uri("http://192.241.238.82/2014/moodle/login/signup.php?", UriKind.Absolute)
            };
            TiltEffect.SetIsTiltEnabled(hyperlinkButton, true);

            CustomMessageBox mensagem = new CustomMessageBox();
            mensagem.Caption = "Deseja criar uma nova conta?";
            mensagem.Content = hyperlinkButton;
            mensagem.Message = "Você precisa ter uma conta Moodle na Ulbra Torres para poder logar neste aplicativo," +
                                " seu nível de usuário precisa ser de 'aluno' e você precisa estar inscrito em pelo menos" +
                                " um curso. Se você ainda não tem uma conta Moodle, crie uma agora mesmo acessando o link abaixo:";

            mensagem.LeftButtonContent = "Ok, entendi";
            //mensagem.RightButtonContent = "right";
            mensagem.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        //do anything
                        break;
                    case CustomMessageBoxResult.RightButton:
                        //do anything
                        break;
                    default:
                        break;
                }
            };
            mensagem.Show();
        }

        #endregion
        
    }
}