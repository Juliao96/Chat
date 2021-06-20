using Chat.Common;
using Softblue.Mvvm;
using System.Collections.ObjectModel;

namespace Chat.Client
{
    class ChatViewModel : Bindable
    {
        // Cliente do chat.
        // Клиент чата.
        public ChatClient chatClient;

        // Serviços de janela.
        // Оконные службы.
        public IChatWindowServices WindowServices { get; set; }

        // Lista de nomes de membros conectados.
        // Список имен подключенных участников.
        // É do tipo ObservableCollection para que as mudanças nos elementos sejam refletidas na interface gráfica.
        // Это тип ObservableCollection, поэтому изменения элементов отражаются в графическом интерфейсе.
        private ObservableCollection<string> names;
        public ObservableCollection<string> Names
        {
            get { return names; }
            set { SetValue(ref names, value); }
        }

        // Mensagem a ser enviada.
        // Сообщение для отправки.
        private string message;
        public string Message
        {
            get { return message; }
            set { SetValue(ref message, value); }
        }

        // Mensagens do chat.
        // Сообщения чата.
        private string messages;
        public string Messages
        {
            get { return messages; }
            set { SetValue(ref messages, value); }
        }

        // Título da janela.
        // Заголовок окна.
        private string title;
        public string Title
        {
            get { return title; }
            set { SetValue(ref title, value); }
        }

        // Comando de envio de mensagem.
        // Команда отправки сообщения.
        public Command SendCommand { get; set; }

        // ViewModel implementa o padrão singleton.
        // ViewModel реализует одноэлементный шаблон.
        private static ChatViewModel current;
        public static ChatViewModel Current
        {
            get
            {
                if (current == null)
                {
                    current = new ChatViewModel();
                }
                return current;
            }
        }

        // Construtor privado.
        // Частный конструктор.
        private ChatViewModel()
        {
            Names = new ObservableCollection<string>();
            SendCommand = new Command(Send);
        }

        // Inicializa o ViewModel.
        // Инициализируем ViewModel.
        public void Initialize(ChatClient chatClient)
        {
            this.chatClient = chatClient;

            // Customiza o título com o nome de quem se conectou.
            // Настройте заголовок с именем входа.
            Title = string.Format("Chat - {0}", chatClient.Name);

            // Registro nos eventos necessários.
            // Авторизуемся в необходимых событиях.
            chatClient.InputHandler.GetMembersResponse += OnGetMembersResponse;
            chatClient.InputHandler.MessageReceived += OnMessageReceived;
            chatClient.InputHandler.MemberEntered += OnMemberEntered;
            chatClient.InputHandler.MemberLeft += OnMemberLeft;
            chatClient.InputHandler.MemberCanLeave += OnMemberCanLeave;
            chatClient.InputHandler.ServerDisconnecting += OnServerDisconnecting;

            // Solicita a lista de membros.
            // Запрос списка участников.
            chatClient.OutputHandler.SendGetMembersCommand();
        }

        // Algum membro mandou uma mensagem.
        // Какой-то участник отправил сообщение.
        void OnMessageReceived(object sender, MessageEventArgs e)
        {
            if (WindowServices != null)
            {
                WindowServices.RunOnUIThread(() =>
                {
                    // Mostra a mensagem na tela.
                    ShowMessage(e.Message);
                });
            }
        }

        // O servidor enviou a lista de membros.
        // Сервер отправил список участников.
        void OnGetMembersResponse(object sender, MembersEventArgs e)
        {
            if (WindowServices != null)
            {
                WindowServices.RunOnUIThread(() =>
                {
                    // Atribui a lista à coleção 'Nomes'.
                    // Присваиваем список к коллекции "Имена".
                    Names = new ObservableCollection<string>(e.Members);
                });
            }
        }

        // Um novo membro entrou.
        // К нам присоединился новый участник.
        void OnMemberEntered(object sender, MemberEventArgs e)
        {
            if (WindowServices != null)
            {
                WindowServices.RunOnUIThread(() =>
                {
                    // Adiciona o nome na lista de nomes.
                    // Добавить имя в список имен.
                    Names.Add(e.Name);

                    // Mostra mensagem avisando.
                    // Показать предупреждение о сообщении.
                    ShowMessage(e.Name + " вошел в чат");
                });
            }
        }

        // Um membro saiu.
        // Участник ушел.
        void OnMemberLeft(object sender, MemberEventArgs e)
        {
            if (WindowServices != null)
            {
                WindowServices.RunOnUIThread(() =>
                {
                    // Remove o nome da lista de nomes.
                    // Удаляем имя из списка имен.
                    Names.Remove(e.Name);

                    // Mostra mensagem avisando.
                    // Показать предупреждение о сообщении.
                    ShowMessage(e.Name + " покинул чат");
                });
            }
        }

        // Indica que o servidor autorizou este cliente a sair do chat.
        // Указывает, что сервер разрешил этому клиенту покинуть чат.
        private void OnMemberCanLeave(object sender, MemberEventArgs e)
        {
            // Para a thread que lê dados da stream.
            // Остановить поток, который читает данные из stream.
            e.StopInputHandler = true;

            // Desconecta o cliente.
            // Отключаем клиента.
            chatClient.Disconnect();

            if (WindowServices != null)
            {
                // Fecha a janela do chat.
                // Закройте окно чата.
                WindowServices.RunOnUIThread(() => WindowServices.CloseWindow());
            }
        }

        // O servidor está se deconectando.
        // Сервер отключается.
        private void OnServerDisconnecting(object sender, BaseEventArgs e)
        {
            // Inicia o processo de desconexão do cliente.
            // Запускаем процесс отключения клиента.
            // Quando o servidor avisa que vai se desconectar, todos os cliente se desconectam antes.
            // Когда сервер сообщает, что он отключится, сначала отключаются все клиенты.
            LeaveRoom();
        }

        // O cliente está iniciando o processo de sair do chat.
        // Клиент начинает процесс выхода из чата.
        public void LeaveRoom()
        {
            // Avisa o servidor que o cliente está saindo.
            // Уведомляем сервер об уходе клиента.
            chatClient.OutputHandler.SendMemberLeavingCommand(chatClient.Name);
        }

        // Envia uma mensagem.
        // Послать сообщение.
        public void Send()
        {
            if (!string.IsNullOrEmpty(Message))
            {
                // Envia a mensagem ao servidor.
                // Отправляем сообщение на сервер.
                chatClient.OutputHandler.SendMessageCommand(Message);

                // Limpa o texto depois de enviado.
                // Очистить текст после отправки.
                Message = "";
            }
        }

        // Mostra a mensagem na tela.
        // Выводим сообщение на экран.
        void ShowMessage(string message)
        {
            if (string.IsNullOrEmpty(messages))
            {
                // Se for a primeira mensagem, apenas mostra.
                // Если это первое сообщение, оно просто отображается.
                Messages = message;
            }
            else
            {
                // Se não for a primeira mensagem, coloca uma quebra de linha no início.
                // Если не первое сообщение, в начале ставим разрыв строки.
                Messages += "\n" + message;
            }

            // Faz o scroll até o final das mensagens.
            // Прокручиваем до конца сообщений.
            WindowServices.ScrollMessagesToEnd();
        }
    }
}
