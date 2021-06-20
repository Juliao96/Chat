using System;

namespace Chat.Common
{
    // Comandos possíveis de serem enviados pelo socket.
    // Команды можно отправлять через сокет.
    public enum ChatCommandType
    {
        EnterRoom,                  // Клиент хочет войти в комнату. // Cliente quer entrar na sala.
        EnterRoomResponse,          // Ответ сервера, информирующий, может ли клиент присоединиться. // Resposta do servidor, informando se o cliente pode entrar.
        GetMembers,                 // Получить список подключенных участников. // Obter lista de membros conectados.
        GetMembersResponse,         // Ответ со списком подключенных участников.// Resposta com a lista de membros conectados.
        SendMessage,                // Отправляем сообщение всем в чате. // Enviar uma mensagem para todos no chat.
        MessageReceived,            // Сервер получил запрос на отправку сообщения от участника. // O servidor recebeu um pedido de envio de mensagem de um membro.
        MemberEntered,              // К чату присоединился новый участник. // Um novo membro entrou no chat.
        MemberLeft,                 // Участник покинул чат. // Um membro saiu do chat.
        MemberLeaving,              // Участник заявляет, что хочет уйти. // Um membro está informando que deseja sair.
        MemberCanLeave,             // Ответ сервера, указывающий, что участник может уйти. // Resposta do servidor indicando que o membro pode sair.
        ServerDisconnecting         // Сервер в процессе отключения. // O servidor está em processo de desconexão.
    }

    // Representa um comando específico.
    // Представляет конкретную команду.
    public class ChatCommand
    {
        // Tipo de comando.
        // Тип команды.
        public ChatCommandType Type { get; set; }

        // Parâmetro do comando.
        // Командный параметр.
        public string Param { get; set; }

        public ChatCommand(ChatCommandType type, string param)
        {
            this.Type = type;
            this.Param = param;
        }

        // Cria um comando a partir de uma string (o formato é "<tipo>|<parâmetro>")
        // Создаем команду из строки (формат "<тип> | <параметр>")
        public static ChatCommand Parse(string commandStr)
        {
            int delimiterPos = commandStr.IndexOf('|');
            string typeStr = commandStr.Substring(0, delimiterPos);
            ChatCommandType type = (ChatCommandType)Enum.Parse(typeof(ChatCommandType), typeStr);
            string param = commandStr.Substring(delimiterPos + 1);

            return new ChatCommand(type, param);
        }

        // Converte o comando para um formato de string.
        // Преобразование команды в строковый формат.
        public override string ToString()
        {
            return Type + "|" + Param;
        }
    }
}
