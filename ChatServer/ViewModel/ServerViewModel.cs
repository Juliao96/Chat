using Softblue.Mvvm;

namespace Chat.Server
{
    class ServerViewModel : Bindable
    {
        // Servidor do chat.
        // Чат-сервер.
        ChatServer server;

        // Comandos chamados pelos botões 'Conectar' e 'Desconectar'
        // Команды, вызываемые кнопками «Подключить» и «Отключить»
        public Command ConnectCommand { get; set; }
        public Command DisconnectCommand { get; set; }

        // Porta do servidor.
        // Порт сервера.
        // A property é do tipo string para evitar que uma porta não especificada mostre o valor 0.
        // Свойство имеет строковый тип, чтобы предотвратить отображение значения 0 для неуказанного порта.
        string port;
        public string Port
        {
            get { return port; }
            set
            {
                port = value;
                int portInt = int.Parse(port);

                if (portInt < Settings.Default.MinPort)
                {
                    // A porta não por ser menor do que MinPort
                    // Порт не меньше MinPort
                    AddError("Порт не может быть меньше " + Settings.Default.MinPort);
                }
                else if (portInt > Settings.Default.MaxPort)
                {
                    // A porta não pode ser maior do que MaxPort
                    // Порт не может быть больше MaxPort
                    AddError("Порт не может быть больше " + Settings.Default.MaxPort);
                }
                else
                {
                    // Tudo certo com a validação.
                    // С валидацией все нормально.
                    RemoveErrors();
                    Settings.Default.Port = value;
                }
            }
        }

        string ip;
        public string Server
        {
         
            get { return ip; }
            set
            {
                ip = value;
                if(ip=="localhost")
                {
                    ip = "127.0.0.1";
                }

                Settings.Default.Server = value;
            }
        }

        // Indica se o servidor está conectado.
        // Указывает, подключен ли сервер.
        private bool connected;
        public bool Connected
        {
            get { return connected; }
            set
            {
                SetValue(ref connected, value);
                OnPropertyChanged("NotConnected");
                OnPropertyChanged("CanConnect");
            }
        }

        // Indica se é possível conectar.
        // Указывает, возможно ли подключение.
        public bool CanConnect
        {
            get
            {
                // Só é possível conectar se não houver erros de validação e se a conexão já não tiver sido feita.
                // Подключение возможно только в том случае, если нет ошибок проверки и если подключение еще не было выполнено.
                return !HasErrors && !Connected;
            }
        }

        // Indica se o servidor não está conectado.
        // Указывает, не подключен ли сервер.
        public bool NotConnected
        {
            get { return !Connected; }
        }

        // Implementação do padrão singleton.
        // Реализация паттерна Singleton.
        private static ServerViewModel current;
        public static ServerViewModel Current
        {
            get
            {
                if (current == null)
                {
                    current = new ServerViewModel();
                }
                return current;
            }
        }

        // Construtor.
        // Конструктор.
        private ServerViewModel()
        {
            // Cria o servidor do chat.
            // Создаем чат-сервер.
            server = new ChatServer();

            // Atribui a porta a partir dos settings.
            // Назначить порт из настроек.
            Port = Settings.Default.Port;

            // Cria os comandos.
            // Создаем команды.
            ConnectCommand = new Command(Connect);
            DisconnectCommand = new Command(Disconnect);

            // Registro no evento disparado quando há mudança nos erros de validação.
            // Вход в систему по событию, запускаемому при изменении ошибок проверки.
            ErrorsChanged += (s, e) => OnPropertyChanged("CanConnect");

            // Registro nos eventos de servidor conectado e desconectado.
            // Вход в систему событий подключенного и отключенного сервера.
            server.Connected += (s, e) => Connected = true;
            server.Disconnected += (s, e) => Connected = false;
        }

        private void Connect()
        {
            // Conecta o servidor.
            // Подключаем сервер.
            server.Connect(Server, int.Parse(Port));
        }

        public void Disconnect()
        {
            // Desconecta o servidor.
            // Отключаем сервер.
            server.Disconnect();
        }
    }
}
