using System;
using System.Windows;
using System.Windows.Input;

namespace Chat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window, ILoginWindowServices
    {
        public LoginWindow()
        {
            InitializeComponent();

            // Associa a instância do ModelView ao DataContext da janela.
            // Привязывает экземпляр ModelView к DataContext окна.
            LoginViewModel viewModel = LoginViewModel.Current;
            DataContext = viewModel;

            // Atribui o WindowServices ao ViewModel (isto permite que o ViewModel se comunique com a janela sem conhecer a view).
            // Назначьте WindowServices ViewModel (это позволяет ViewModel взаимодействовать с окном, не зная представления).
            viewModel.WindowServices = this;
        }

        // Permite apenas caracteres numéricos como valor para a porta.
        // Допустим только числовые символы в качестве значения для порта.
        private void txtPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0)
            {
                if (!char.IsDigit(e.Text, e.Text.Length - 1))
                {
                    e.Handled = true;
                }
            }
        }

        // Mostra uma caixa de diálogo de erro de conexão.
        // Показываем диалог ошибки подключения.
        public void ShowErrorConnectionDialog(string errorMessage)
        {
            MessageBox.Show(this, errorMessage, "Erro de conexão", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Abre a janela do chat.
        // Открываем окно чата.
        public void OpenChatWindow(ChatClient chatClient)
        {
            ChatWindow window = new ChatWindow();

            // Inicializa o ViewModel da janela do chat.
            // Инициализируем ViewModel окна чата.
            ChatViewModel.Current.Initialize(chatClient);
            
            window.Show();
            Close();
        }

        public void RunOnUIThread(Action action)
        {
            // Executa o código usando a thread da interface gráfica.
            // Выполняем код, используя thread графического интерфейса.
            Dispatcher.Invoke(action);
        }
    }
}
