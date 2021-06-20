using Chat.Common;
using Softblue.Mvvm;
using System.Threading.Tasks;

namespace Chat.Client
{
    class LoginViewModel : Bindable
    {
        // Cliente do chat.
        // Клиент чата.
        ChatClient chatClient;

        // Nome do servidor.
        // Имя сервера.
        private string server;
        public string Server
        {
            get { return server; }
            set
            {
                SetValue(ref server, value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    // O servidor não pode ser vazio.
                    // Сервер не может быть пустым.
                    AddError("Предоставить сервер");
                }
                else
                {
                    // Tudo ok com a validação.
                    // Все в порядке с валидацией.
                    RemoveErrors();
                    Settings.Default.Server = value;
                }
            }
        }

        // Porta para se conectar.
        // Порт для подключения.
        private string port;
        public string Port
        {
            get { return port; }
            set
            {
                SetValue(ref port, value);
                if (string.IsNullOrWhiteSpace(value))
                {
                    // A porta não pode ser vazia.
                    // Порт не может быть пустым.
                    AddError("Предоставить порт");
                }
                else
                {
                    // Tudo ok com a validação.
                    // Все в порядке с валидацией.
                    RemoveErrors();
                    Settings.Default.Port = value;
                }
            }
        }


        // Nome do cliente no chat.
        // Имя клиента в чате.
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                SetValue(ref name, value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    // O nome não pode ser vazio.
                    // Имя не может быть пустым.
                    AddError("Укажите имя");
                }
                else
                {
                    // Tudo ok com a validação.
                    // Все в порядке с валидацией.
                    RemoveErrors();
                    Settings.Default.Name = value;
                }
            }
        }

        // Comando de conexão.
        // Команда подключения.
        public Command ConnectCommand { get; set; }

        // Indica se a conexão pode ser feita (apenas se não há erros de validação e se a conexão já não está sendo feita)
        // Указывает, можно ли установить соединение (только если нет ошибок проверки и если соединение еще не установлено)
        public bool CanConnect { get { return !HasErrors && NotTryingConnect; } }

        // Indica se a conexão não está sendo feita.
        // Указывает, не выполняется ли соединение.
        private bool notTryingConnect;
        public bool NotTryingConnect
        {
            get { return notTryingConnect; }
            set
            {
                SetValue(ref notTryingConnect, value);
                OnPropertyChanged("CanConnect");
            }
        }

        // Serviços de janela.
        // Оконные службы.
        public ILoginWindowServices WindowServices { get; set; }

        // ViewModel implementa padrão singleton.
        // ViewModel реализует singleton шаблон 
        static LoginViewModel current;
        public static LoginViewModel Current
        {
            get
            {
                if (current == null)
                {
                    current = new LoginViewModel();
                }
                return current;
            }
        }

        // Construtor privado.
        // Частный конструктор.
        private LoginViewModel()
        {
            // Instancia o cliente do chat.
            // Создание экземпляра чат-клиента.
            chatClient = new ChatClient();

            // Lê os dados dos settings.
            // Считываем данные из настроек.
            Server = Settings.Default.Server;
            Port = Settings.Default.Port;
            Name = Settings.Default.Name;

            NotTryingConnect = true;

            // Registra o comando de conexão.
            // Зарегистрируем команду подключения.
            ConnectCommand = new Command(ConnectAsync);

            // Registro no evento de mudança nos erros de validação.
            // Регистрируемся в событии изменения при ошибках проверки.
            ErrorsChanged += (s, e) => OnPropertyChanged("CanConnect");
        }

        // Inicia a conexão de forma assíncrona.
        // Запускаем соединение асинхронно.
        private async void ConnectAsync()
        {
            NotTryingConnect = false;

            // Faz a conexão no servidor. A presenta do await faz com que esta chamada seja assíncrona.
            // Подключаемся к серверу. Наличие await делает этот вызов асинхронным.
            bool connected = await Task<bool>.Factory.StartNew(() => chatClient.Connect(Server, int.Parse(Port), Name));

            if (!connected)
            {
                // Não foi possível conectar. Mostra o erro para o usuário.
                // Невозможно подключиться. Показать ошибку пользователю.
                if (WindowServices != null)
                {
                    WindowServices.ShowErrorConnectionDialog("Сервер не найден");
                }

                NotTryingConnect = true;
            }
            else
            {
                // A conexão foi feita. Envia o pedido de entrada na sala e aguarda resposta.
                // Соединение установлено. Отправьте заявку на вход в комнату и ждите ответа.
                chatClient.InputHandler.EnterRoomResponse += OnEnterRoomResponse;
                chatClient.OutputHandler.SendEnterRoomCommand(Name);
            }
        }

        private void OnEnterRoomResponse(object sender, EnterRoomResponseEventArgs e)
        {
            // O servidor mandou uma resposta sobre o pedido de entrada na sala.
            // Сервер отправил ответ на запрос на вход в комнату.
            if (WindowServices != null)
            {
                WindowServices.RunOnUIThread(() =>
                {
                    if (e.Valid )
                    {
                        // A entrada foi autorizada. Abre a janela do chat.
                        // Вход авторизован. Откройте окно чата.
                        WindowServices.OpenChatWindow(chatClient);
                    }
                    else
                    {
                        // A entrada foi negada. Mostra o erro.
                        // Въезд запрещен. Показывает ошибку.
                        WindowServices.ShowErrorConnectionDialog(e.Error);

                        // Desconecta o cliente.
                        // Отключаем клиента.
                        chatClient.Disconnect();
                        NotTryingConnect = true;

                        // Para a thread que lê dados do canal.
                        // Остановить thread, который читает данные из канала.
                        e.StopInputHandler = true;
                    }
                });
            }
        }
    }
}
