using Chat.Common;
using System.Net.Sockets;

namespace Chat.Client
{
    public class ChatClient
    {
        // Cliente do socket.
        // Клиент сокета
        TcpClient tcpClient;

        // InputHandler e OutputHandler, que gerenciam os dados da stream.
        // InputHandler и OutputHandler, которые управляют данными потока.
        public InputHandler InputHandler { get; private set; }
        public OutputHandler OutputHandler { get; private set; }

        // Nome usado pelo cliente no chat.
        // Имя, используемое клиентом в чате.
        public string Name { get; set; }

        // Faz a conexão no servidor.
        // Подключаемся к серверу.
        public bool Connect(string server, int port, string name)
        {
            this.Name = name;
            try
            {
                tcpClient = new TcpClient(server, port);
                InputHandler = new InputHandler(tcpClient.GetStream());
                OutputHandler = new OutputHandler(tcpClient.GetStream());

                // Inicia a thread do InputHandler, que fica aguardando a chegada de dados na stream.
                // Запускаем thread InputHandler, который ожидает поступления данных в поток.
                InputHandler.Start();

                // Se deu tudo certo, retorna true, indicando que a conexão foi realizada.
                // Если все сработало, возвращает истину, указывая на то, что соединение было установлено.
                return true;
            }
            catch (SocketException)
            {
                // Se gerar exceção, a conexão não pode ser estabelecida com o servidor.
                // Если выдается исключение, соединение с сервером невозможно установить.
                return false;
            }
        }

        public void Disconnect()
        {
            // Desconecta o socket.
            // Отключить сокет.
            tcpClient.Close();
        }
    }
}
