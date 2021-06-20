using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Chat.Server
{
    class ChatServer
    {
        // Eventos de conectado e desconectado.
        // Подключенные и отключенные события.
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        readonly object syncRunning = new object();
        bool running;

        // Servidor de socket.
        // Сокет-сервер.
        TcpListener tcpListener;

        // Lista de clientes conectados no chat.
        // Список клиентов, подключенных к чату.
        public List<Member> Members { get; set; }

        public ChatServer()
        {
            Members = new List<Member>();
        }

        // Conecta na porta especificada.
        // Подключаемся к указанному порту.
        public void Connect(string ip, int port)
        {
            // A conexão é feita usando uma thread a parte.
            // Подключение осуществляется с помощью отдельного thread.
            Task.Factory.StartNew(() => HandleConnection(ip, port));
        }

        private void HandleConnection(string ip, int port)
        {

            // Abre o servidor do scoket.
            // Открываем сокет-сервер.
            IPAddress enderecoIP = IPAddress.Parse(ip);
            tcpListener = new TcpListener(enderecoIP, port);
            tcpListener.Start();

            lock (syncRunning)
            {
                running = true;
            }

            if (Connected != null)
            {
                // Dispara o evento dizendo que o servidor foi conectado.
                // Запускаем событие, сообщающее, что сервер подключился.
                Connected(this, EventArgs.Empty);
            }

            while (true)
            {
                if (tcpListener.Pending())
                {
                    // Se houver um pedido de conexão pendente, conecta e cria um objeto Member para responder a requisição.
                    // Если есть ожидающий запрос на соединение, подключитесь и создайте объект Member для ответа на запрос.
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Member member = new Member(this, tcpClient);
                    member.HandleMemberInteraction();
                }
                else
                {
                    // Se não houver um pedido de conexão pendente, verifica se é para terminar o loop e aguarda 1s.
                    // Если нет ожидающего запроса на соединение, проверяем, должен ли он завершить цикл, и ждем 1 с.
                    lock (syncRunning)
                    {
                        if (!running)
                        {
                            break;
                        }
                    }

                    Thread.Sleep(1000);
                }
            }

            // O loop terminou, então para o servidor do socket.
            // Цикл закончился, значит, к серверу сокетов.
            tcpListener.Stop();

            if (Disconnected != null)
            {
                // Dispara o evento informando que o servidor foi desconectado.
                // Запускает событие, информирующее вас о том, что сервер отключен.
                Disconnected(this, EventArgs.Empty);
            }
        }

        // Desconecta o servidor.
        // Отключаем сервер.
        public void Disconnect()
        {
            // Envia um comando para os membros informando que o servidor vai se desconectar.
            // Отправляем участникам команду, информирующую, что сервер отключится.
            lock (Members)
            {
                foreach (Member member in Members)
                {
                    member.SendServerDisconnectingCommand();
                }

            }

            // Fica em um loop aguardando até que todos os clientes se desconectem do servidor.
            // Он находится в цикле, ожидая, пока все клиенты не отключатся от сервера.
            while (true)
            {
                lock (Members)
                {
                    if (Members.Count == 0)
                    {
                        break;
                    }
                }
                Thread.Sleep(500);
            }

            // Agora que não há mais membros, o servidor pode ser encerrado.
            // Теперь, когда участников больше нет, сервер можно выключить.
            lock (syncRunning)
            {
                running = false;
            }
        }
    }
}
