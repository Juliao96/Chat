using System.Windows;
using System.Windows.Input;

namespace Chat.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Atribui a instância do ViewModel ao DataContext da janela
            // Присваиваем экземпляр ViewModel к DataContext окна
            ServerViewModel viewModel = ServerViewModel.Current;
            DataContext = viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Quando a janela é carregada, coloca o cursor no final do texto que representa a porta.
            // Когда окно загружено, поместите курсор в конец текста, представляющего порт.
            txtPort.CaretIndex = txtPort.Text.Length;
        }

        private void txtPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permite que apenas caracteres numéricos sejam digitados como valor para a porta do servidor.
            // Разрешить ввод только числовых символов в качестве значения для порта сервера.
            if (e.Text.Length > 0)
            {
                if (!char.IsDigit(e.Text, e.Text.Length - 1))
                {
                    e.Handled = true;
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Quando a janela for fechada, inicia o processo de desconexão do servidor.
            // При закрытии окна запускает процесс отключения от сервера.
            ServerViewModel.Current.Disconnect();
        }

        private void txtPort_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
