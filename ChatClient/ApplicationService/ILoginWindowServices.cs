using System;

namespace Chat.Client
{
    // Serviços para o LoginViewModel se comunicar com a tela de login.
    // Сервисы для LoginViewModel для связи с экраном входа в систему.
    interface ILoginWindowServices
    {
        void ShowErrorConnectionDialog(string errorMessage);
        void OpenChatWindow(ChatClient chatClient);
        void RunOnUIThread(Action action);
    }
}
