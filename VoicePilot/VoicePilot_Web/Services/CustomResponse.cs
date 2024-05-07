using VoicePilot.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using VoicePilot.Models.Response;
using VoicePilot.Utilities;


namespace VoicePilot.Services
{
    public class CustomResponse
    {

        private readonly ILogger<CustomResponse> _logger;
        private readonly IHubContext<MessageRelayHub> _messageRelayHubContext;
        private readonly Kernel _kernel;
        private string _pluginsDirectory = string.Empty;
        private readonly int DELAY = 0;

        public string PluginName { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;


        public CustomResponse(ILogger<CustomResponse> logger, [FromServices] Kernel kernel,
        [FromServices] IHubContext<MessageRelayHub> messageRelayHubContext)
        {
            _logger = logger;
            _kernel = kernel;
            _messageRelayHubContext = messageRelayHubContext;
            _pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        }

        // Method to handle the GetAsync request
        public async Task<FunctionResult> GetAsync(string whatAbout, Dictionary<string, string> variablesContext)
        {
            var arguments = new KernelArguments();

            foreach (var item in variablesContext)
            {
                arguments[item.Key] = item.Value;
            }

            var result = await _kernel.InvokeAsync(_kernel.Plugins[this.PluginName][this.FunctionName], arguments);

            Console.Write(result);
            return result;


        }

        public async Task GetStreamingAsync(string whatAbout, Dictionary<string, string> variablesContext)
        {
            var arguments = new KernelArguments();

            foreach (var item in variablesContext)
            {
                arguments[item.Key] = item.Value;
            }

            await StreamResponseToClient("", whatAbout, arguments);

        }

        // Method to read the prompt from a function in a plugin
        public async Task<string> ReadPrompt(string functionName, string plugin)
        {
            string? promptStream = await ReadFunction.Read(Path.Combine(_pluginsDirectory, plugin, functionName),
                FunctionFileType.Prompt);
            return promptStream!;
        }

        // Method to stream the response to the client
        private async Task<MessageResponse> StreamResponseToClient(string chatId, string whatAbout, KernelArguments arguments)
        {

            MessageResponse messageResponse = new MessageResponse
            {
                State = "Start",
                Content = "",
                WhatAbout = whatAbout
            };

            await foreach (StreamingChatMessageContent contentPiece in _kernel.InvokeStreamingAsync<StreamingChatMessageContent>(_kernel.Plugins[this.PluginName][this.FunctionName], arguments))
            {
                await this.UpdateMessageOnClient(messageResponse, chatId);
                messageResponse.State = "InProgress";

                if (!string.IsNullOrEmpty(contentPiece.Content))
                {
                    messageResponse.Content += contentPiece.Content;
                    await this.UpdateMessageOnClient(messageResponse, chatId);
                    Console.Write(contentPiece.Content);
                    await Task.Delay(DELAY);
                }
            }

            //var response = await _kernel.InvokeAsync(_kernel.Plugins[this.PluginName][this.FunctionName], arguments);
            //messageResponse.Content = response.GetValue<string>()!;
            messageResponse.State = "End";
            await this.UpdateMessageOnClient(messageResponse, chatId);
            return messageResponse;
        }

        /// <summary>
        /// Update the response on the client.
        /// </summary>
        /// <param name="message">The message</param>
        private async Task UpdateMessageOnClient(MessageResponse message, string chatId)
        {
            await this._messageRelayHubContext.Clients.All.SendAsync("ReceiveMessageUpdate", message);
        }



    }
}
