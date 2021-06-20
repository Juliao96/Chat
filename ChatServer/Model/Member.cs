using System;
using System.Net.Sockets;
using Chat.Common;
using System.Collections.Generic;

namespace Chat.Server
{
    // Representa um cliente conectado no chat.
    // Представляет клиента, подключенного к чату.
    class Member
    {
        TcpClient tcpClient;
        ChatServer chatServer;

        // Nome do membro
        // Имя участника
        string name;

        // Handler para gerenciar os comandos que chegam ao membro e são enviados ao cliente do chat.
        // Обработчик для управления командами, которые достигают участника и отправляются клиенту чата.
        InputHandler inputHandler;
        OutputHandler outputHandler;

        public Member(ChatServer chatServer, TcpClient tcpClient)
        {
            this.chatServer = chatServer;
            this.tcpClient = tcpClient;

            inputHandler = new InputHandler(tcpClient.GetStream());
            outputHandler = new OutputHandler(tcpClient.GetStream());
        }

        public void HandleMemberInteraction()
        {
            // Registra os eventos necessários.
            // Записываем необходимые события.
            inputHandler.EnterRoom += OnEnterRoom;
            inputHandler.GetMembers += OnGetMembers;
            inputHandler.SendMessage += OnSendMessage;
            inputHandler.MemberLeaving += OnMemberLeaving;

            // Inicia o input handler, que fica aguardando a chegada de comandos do cliente na stream.
            // Запускаем обработчик ввода, который ожидает поступления клиентских команд в поток.
            inputHandler.Start();
        }

        // Um cliente quer entrar na sala.
        // Клиент хочет войти в комнату.
        void OnEnterRoom(object sender, MemberEventArgs e)
        {
            // Define o seu nome.
            // Определите ваше имя.
            name = e.Name;

            bool valid = true;
            string error = null;

            lock (chatServer.Members)
            {
                // Verifica se já não existe um membro com o mesmo nome.
                // Проверяем, не существует ли уже члена с таким именем.
                foreach (Member member in chatServer.Members)
                {
                    if (member != this && member.name == name)
                    {
                        valid = false;
                        error = "Имя " + name + " уже есть в чате";
                        break;
                    }
                }
            }

            // Envia uma resposta ao cliente, dizendo se ele pode ou não entrar. Em caso negativo, envia o erro.
            // Отправляет ответ клиенту, сообщая, может ли он присоединиться. Если нет, отправьте ошибку.
            outputHandler.SendEnterRoomResponseCommand(valid, error);

            if (valid)
            {
                // Se o cliente pode se conectar, avisa todos os membros existentes que um novo membro está entrando.
                // Если клиент может подключиться, уведомить всех существующих участников о присоединении нового участника.
                lock (chatServer.Members)
                {
                    foreach (Member member in chatServer.Members)
                    {
                        member.outputHandler.SendMemberEnteredCommand(name);
                    }

                    // Adiciona o cliente na lista de membros.
                    // Добавляем покупателя в список участников.
                    chatServer.Members.Add(this);
                }
            }
            else
            {
                // Se o cliente não pode se conectar, indica que a thread de leitura da stream deve ser interrompida.
                // Если клиент не может подключиться, это означает, что поток чтения потока должен быть остановлен.
                e.StopInputHandler = true;
            }
        }

        void OnGetMembers(object sender, BaseEventArgs e)
        {
            // Um novo membro solicita a lista de membros conectados.
            // Новый участник запрашивает список подключенных участников.

            List<string> names = new List<string>();

            lock (chatServer.Members)
            {
                // Cria uma lista com os nomes dos membros.
                // Создаем список имен членов.
                foreach (Member member in chatServer.Members)
                {
                    names.Add(member.name);
                }
            }

            // Envia a lista para o membro que pediu.
            // Отправляем список участнику, который его запросил.
            outputHandler.SendGetMembersResponseCommand(names);
        }

        void OnSendMessage(object sender, MessageEventArgs e)
        {
            // Um membro está mandando uma mensagem.
            // Участник отправляет сообщение.

            // Decora a mensagem com a hora atual e o nome do membro.
            // Украсить сообщение текущим временем и именем участника.
            string time = DateTime.Now.ToString("HH:mm:ss");
            string message = string.Format("[{0}] {1} - {2}", time, name, e.Message);

            lock (chatServer.Members)
            {
                // Envia a mensagem para todos os membros conectados.
                // Отправить сообщение всем подключенным участникам.
                foreach (Member member in chatServer.Members)
                {
                    member.outputHandler.SendMessageReceivedCommand(message);
                }
            }
        }

        void OnMemberLeaving(object sender, MemberEventArgs e)
        {
            // Um membro está saindo do chat.
            // Участник покидает чат.

            lock (chatServer.Members)
            {
                // Remove o membro da lista de membros.
                // Удаляем участника из списка участников.
                chatServer.Members.Remove(this);

                // Avisa os outros membros a respeito da saída.
                // Предупредить других участников о выходе.
                foreach (Member member in chatServer.Members)
                {
                    member.outputHandler.SendMemberLeftCommand(name);
                }
            }

            // Avisa o membro que agora ele está autorizado a se desconectar.
            // Уведомляем участника о том, что теперь он имеет право отключиться.
            outputHandler.SendMemberCanLeaveCommand(name);

            // Indica que a thread de leitura da stream deve ser interrompida.
            // Указывает, что поток чтения потока должен быть остановлен.
            e.StopInputHandler = true;
        }

        public void SendServerDisconnectingCommand()
        {
            // O servidor está sendo desconectado, então avisa o membro.
            // Сервер отключается, поэтому уведомляем участника.
            outputHandler.SendServerDisconnectingCommand();
        }
    }
}
