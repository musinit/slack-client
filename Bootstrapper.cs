using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;
using Slack.Client.Settings;

namespace Slack.Client
{
    public static class Bootstrapper
    {
        public static IServiceCollection AddSlackClient(this IServiceCollection services, Action<SlackOptions> options)
        {
            var slackOptions = new SlackOptions();
            options(slackOptions);
            services.Configure<SlackOptions>(o =>
                {
                    
                    o.ClientId = slackOptions.ClientId;
                    o.ClientSecret = slackOptions.ClientSecret;
                    o.AppToken = slackOptions.AppToken;
                    o.SlackApi = slackOptions.SlackApi;
                });
            services.AddHttpClient<ISlackClient, SlackClient>(client => {
                client.BaseAddress = new Uri(slackOptions.SlackApi);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", slackOptions.AppToken);
                });
            services.AddTransient<ISlackClient, SlackClient>();

            return services;
        }
    }
}
