using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using VoicePilot.Options;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace VoicePilot.Extensions
{
    internal static class SemanticKernelExtensions
    {
        /// <summary>
        /// Delegate to register plugins with a Semantic Kernel
        /// </summary>
        public delegate Task RegisterPluginsWithKernel(IServiceProvider sp, Kernel kernel);

        /// <summary>
        /// Add Semantic Kernel services
        /// </summary>
        internal static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, string serviceID = "default")
        {

            // Semantic Kernel
            services.AddScoped<Kernel>(sp =>
            {
                IKernelBuilder builder = Kernel.CreateBuilder();
                builder.Services.AddLogging(c => c.AddConsole().SetMinimumLevel(LogLevel.Information));
                builder.WithCompletionBackend(sp.GetRequiredService<IOptions<AIServiceOptions>>().Value, serviceID);
                builder.Services.AddHttpClient();
                Kernel kernel = builder.Build();

                sp.GetRequiredService<RegisterPluginsWithKernel>()(sp, kernel);
                return kernel;
            });

            // Register Plugins
            services.AddScoped<RegisterPluginsWithKernel>(sp => RegisterPluginsAsync);

            return services;

        }

        /// <summary>
        /// Register the plugins with the kernel.
        /// </summary>
        private static Task RegisterPluginsAsync(IServiceProvider sp, Kernel kernel)
        {
            // Semantic Plugins
            ServiceOptions options = sp.GetRequiredService<IOptions<ServiceOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.SemanticPluginsDirectory))
            {
                foreach (string pluginDir in Directory.GetDirectories(options.SemanticPluginsDirectory))
                {
                    try
                    {
                        kernel.ImportPluginFromPromptDirectory(pluginDir);
                    }
                    catch (KernelException e)
                    {
                        var logger = kernel.LoggerFactory.CreateLogger(nameof(Kernel));
                        logger.LogError("Could not load skill from {Directory}: {Message}", pluginDir, e.Message);
                    }


                }
            }

            return Task.CompletedTask;
        }

        ///<summary>
        /// Add the completion backend to the kernel config.
        /// </summary>
        private static IKernelBuilder WithCompletionBackend(this IKernelBuilder kernelBuilder, AIServiceOptions options, string serviceID)
        {
            // Create an HttpClient and include your custom header(s)
            var httpClient = new HttpClient();
            if (options.Type != AIServiceOptions.AIServiceType.Local)
            { 
                httpClient.DefaultRequestHeaders.Add("Authorization", options.Key); 
            }

            // Configure OpenAIClient to use the customized HttpClient
            var clientOptions = new OpenAIClientOptions
            {
                Transport = new HttpClientTransport(httpClient),
            };

            if (serviceID == "Default")
            {
                return options.Type switch
                {
                    AIServiceOptions.AIServiceType.AzureOpenAI
                        => kernelBuilder.AddAzureOpenAIChatCompletion(
                            deploymentName: options.Models.ChatDeploymentName,
                            endpoint: options.Endpoint,
                            serviceId: "AzureOpenAIChat",
                            apiKey: options.Key,
                            httpClient: httpClient),
                    AIServiceOptions.AIServiceType.OpenAI
                        => kernelBuilder.AddOpenAIChatCompletion(options.Models.ChatDeploymentName, options.Endpoint, options.Key),
                    AIServiceOptions.AIServiceType.Local
                       => kernelBuilder.AddOpenAIChatCompletion(options.Models.ChatDeploymentName, options.Endpoint, null),
                    _
                        => throw new ArgumentException($"Invalid {nameof(options.Type)} value in '{AIServiceOptions.PropertyName}' settings."),
                }; ; ;
            }
            else
            {
                //Replace by other DC
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                return options.Type switch
                {
                    AIServiceOptions.AIServiceType.AzureOpenAI
                        => kernelBuilder.AddAzureOpenAIChatCompletion(
                            deploymentName: options.Models.ChatDeploymentName,
                            endpoint: options.Endpoint,
                            serviceId: "AzureOpenAIChat",
                            apiKey: options.Key,
                            httpClient: httpClient),
                    AIServiceOptions.AIServiceType.OpenAI
                        => kernelBuilder.AddOpenAIChatCompletion(options.Models.ChatDeploymentName, options.Endpoint, options.Key),
                    AIServiceOptions.AIServiceType.Local
                        => kernelBuilder.AddOpenAIChatCompletion(modelId: options.Models.ChatDeploymentName, endpoint: new Uri(options.Endpoint), apiKey:null),
                    _
                        => throw new ArgumentException($"Invalid {nameof(options.Type)} value in '{AIServiceOptions.PropertyName}' settings."),
                };
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            }


        }





    }
}
