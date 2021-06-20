using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Chat.Client
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window, IChatWindowServices
    {
        // Indica se a janela deve ser fechada.
        // Указывает, следует ли закрыть окно.
        bool closeWindow = false;
        int cor = 0;

        public ChatWindow()
        {
            InitializeComponent();

            // Atribui a instância do ViewModel ao DataContext da janela.
            // Назначьте экземпляр ViewModel DataContext окна.
            ChatViewModel viewModel = ChatViewModel.Current;
            DataContext = viewModel;
            viewModel.WindowServices = this;
        }

        public void RunOnUIThread(Action action)
        {
            // Executa o código usando a thread da interface gráfica.
            // Выполняем код, используя thread графического интерфейса.
            Dispatcher.Invoke(action);
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            // Faz um tratamento especial caso um ENTER seja digitado na caixa de texto da mensagem.
            // Обеспечивает особую обработку, если в текстовом поле сообщения вводится ENTER.
            if (e.Key != System.Windows.Input.Key.Enter)
            {
                return;
            }

            // Indica que o evento foi tratato.
            // Указывает, что событие было обработано.
            e.Handled = true;

            // Coloca o foco no botão "Enviar" (isto é necessário para que a mensagem seja atualizada no binding do ViewModel).
            // Переводит фокус на кнопку «Отправить» (это необходимо для обновления сообщения в привязке ViewModel).
            FocusManager.SetFocusedElement(this, btnSend);

            // Envia a mensagem.
            // Отправляем сообщение.
            ChatViewModel.Current.Send();

            // Coloca o foco novamente na caixa de texto da mensagem.
            // Возвращает фокус на текстовое поле сообщения.
            txtMessage.Focus();
        }

        public void ScrollMessagesToEnd()
        {
            // Faz o scroll para o final do texto na janela de mensagens.
            // Прокрутите до конца текста в окне сообщения.
            txtMessages.ScrollToEnd();
        }

        public void CloseWindow()
        {
            // Fecha a janela.
            // Закрываем окно.
            closeWindow = true;
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Só fecha a janela realmente se closeWindow for true
            // Фактически закрываем окно, только если closeWindow истинно
            if (!closeWindow)
            {
                // Se closeWindow for false, primeiro faz o processo de saída da sala.
                // Если closeWindow ложно, процесс первым покидает комнату.
                ChatViewModel.Current.LeaveRoom();
                e.Cancel = true;
            }
        }
    }
}
