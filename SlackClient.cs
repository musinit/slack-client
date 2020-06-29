﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Slack.Client.Models;
using Slack.Client.Models.SlackApi;
using Slack.Client.Settings;

namespace Slack.Client
{
    internal class SlackClient : ISlackClient
    {
        private readonly ILogger logger;
        private readonly IHttpClientFactory httpClientFactory;
        private const string forsetiTestChannelId = "CGLG9D9DG";
        private const string tsocialTestChannelId = "CR4P37JL9";
        private SlackOptions Options;

        public SlackClient(IHttpClientFactory httpClientFactory, IOptions<SlackOptions> options, ILogger logger = null)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger ?? new NullLogger<ISlackClient>();
            Options = options.Value;
        }

        public async Task<bool> SendMessageAsync(SendMessageRequest request, bool isTest = true)
        {
            using (var httpClient = httpClientFactory.CreateClient(nameof(ISlackClient)))
            {
                if (request.AppToken != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AppToken);
                }

                var channel = await OpenChannelWithUserAsync(request.AppToken, request.SlackId, httpClient);
                
                SlackResponse slackResponse = await SendMessageInChannelAsync(channel.Id,
                    request.Text,
                    httpClient,
                    isTest,
                    "chat.postMessage",
                    request.CallbackId,
                    request.Attachments,
                    request.Blocks);
                if (!slackResponse.Ok)
                {
                    logger.LogError(slackResponse.Error);
                    return false;
                }
                logger.LogInformation(
                    "Sending slack message for user with Id: {@id}, with message: {@message}, test = {@isTest}", request.SlackId, request.Text, isTest);


                return true;
            }
        }

        public async Task SendResponseMessageAsync(string responseUrl, Block[] blocks)
        {
            using (var httpClient = httpClientFactory.CreateClient(nameof(ISlackClient)))
            {
                httpClient.BaseAddress = new Uri(responseUrl);
                var updateViewSerialized = JsonConvert.SerializeObject(new MessageResponse
                {
                    Text = "Ачивка открыта!",
                    Blocks = blocks
                }); 
                var response = await httpClient.PostAsync("", new StringContent(updateViewSerialized, 
                    Encoding.UTF8, "application/json"));
            }
        }

        public async Task<SlackResponse> UpdateViewAsync<T>(UpdateViewRequest<T> request, string botToken, string requestUrl = "views.publish")
        {
            using (var httpClient = httpClientFactory.CreateClient(nameof(ISlackClient)))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", botToken);
                HttpResponseMessage response;
                SlackResponse slackResponse = new SlackResponse();
                try
                {
                    var updateViewSerialized = JsonConvert.SerializeObject(request);
                    response = await httpClient.PostAsync(requestUrl, new StringContent(updateViewSerialized, 
                        Encoding.UTF8, "application/json"));

                    var responseSource = response.Content.ReadAsStringAsync().Result;
                    slackResponse = JsonConvert.DeserializeObject<SlackResponse>(responseSource);
                    if (!response.IsSuccessStatusCode)
                    {
                        logger.LogError($"Error occured while sending message in slack with code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError($"Couldn't update view for user with Id: {request.UserId}. Error: {ex.Message}");
                    return slackResponse;
                }

                return slackResponse;
            }
        }

        public async Task<SlackUser> GetUserIfExistAsync(string appToken, string email)
        {
            using (var httpClient = httpClientFactory.CreateClient(nameof(ISlackClient)))
            {
                string response = await httpClient.GetStringAsync($"users.lookupByEmail?token={appToken}&email={email}");
                var userResponse = JsonConvert.DeserializeObject<LookUpUserResponse>(response);

                return userResponse.User;
            }
        }

        public async Task<Profile> GetUserProfile(string appToken, string slackId)
        {
            using (var httpClient = httpClientFactory.CreateClient(nameof(ISlackClient)))
            {
                httpClient.BaseAddress = new Uri("https://slack.com");
                var request = new HttpRequestMessage(HttpMethod.Post, "api/users.profile.get");
                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("token", appToken));
                keyValues.Add(new KeyValuePair<string, string>("user", slackId));

                var values = new FormUrlEncodedContent(keyValues);
                var response = await httpClient.PostAsync("api/users.profile.get", values);
                ProfileResponse profileResponse = new ProfileResponse();
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    profileResponse = JsonConvert.DeserializeObject<ProfileResponse>(responseBody);
                }
                else
                {
                    logger.LogError($"Error during fetch slack profile, {response.StatusCode}");
                    return null;
                }

                return profileResponse.Profile;
            }
        }

        public async Task<Profile[]> GetUserProfiles(string appToken, string[] slackIds)
        {
            var result = new List<Profile>();
            foreach (var slackId in slackIds)
            {
                var profile = await GetUserProfile(appToken, slackId);
                result.Add(profile);
            }

            return result.ToArray();
        }

        private async Task<SlackChannel> OpenChannelWithUserAsync(string appToken, string userId, HttpClient httpClient)
        {
            string response;
            try
            {
                response = await httpClient.GetStringAsync($"conversations.open?token={appToken}&users={userId}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Couldn't create channel for user with Id: {userId}. Error: {ex.Message}");
                throw;
            }
            var channelResponse = JsonConvert.DeserializeObject<OpenChannelResponse>(response);
            if (!channelResponse.Ok)
            {
                logger.LogError(channelResponse.Error);
                throw new Exception(channelResponse.Error);
            }

            return channelResponse.Channel;
        }

        private async Task<SlackResponse> SendMessageInChannelAsync(string channelId,
                                                                    string message, 
                                                                    HttpClient httpClient,
                                                                    bool isTest,
                                                                    string requestUrl="chat.postMessage",
                                                                    string callbackId = null, 
                                                                    Attachment[] attachments = null,
                                                                    Block[] blocks = null)
        {
            HttpResponseMessage response;
            var slackMessage = new SlackMessage()
            {
                Channel = isTest ? tsocialTestChannelId : channelId,
                Text = isTest ? message + $" (for {channelId})" : message,
                CallbackId = callbackId,
                Attachments = attachments,
                Blocks = blocks
            };
            try
            {
                //response = await httpClient.GetStringAsync($"chat.postMessage?channel={channelId}&text={message}");
                var messageSerialized = JsonConvert.SerializeObject(slackMessage);
                response = await httpClient.PostAsync(requestUrl, new StringContent(messageSerialized, Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Error occured while sending message in slack with code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Couldn't send message in channel with Id: {channelId}. Error: {ex.Message}");
                throw;
            }
            var slackResponse = JsonConvert.DeserializeObject<SlackResponse>(await response.Content.ReadAsStringAsync());

            return slackResponse;
            
        }

        public async Task<SlackUser[]> GetAllUsers(string botToken)
        {
            var limit = 100;
            var nextCursor = "";
            var result = new List<SlackUser>();
            using (var httpClient = httpClientFactory.CreateClient(nameof(ISlackClient)))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", botToken);
                do
                {
                    string response =
                        await httpClient.GetStringAsync(
                            $"users.list?limit={limit}&cursor={nextCursor.Replace("=", "%3D")}");
                    var userResponse = JsonConvert.DeserializeObject<ListUserResponse>(response);
                    result.AddRange(userResponse.Users.Where(u => !u.IsBot).ToList());
                    nextCursor = userResponse.Metadata.NextCursor;
                } while (nextCursor != "");
                
                return result.ToArray();
            }
            
        }

        public async Task<OAuthResponse> Authorize(string code)
        {
            using (var httpClient = httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri("https://slack.com");
                logger.LogInformation($"Trying to authorize with clientId: {Options.ClientId} and clientSecret: {Options.ClientSecret}");
                Console.WriteLine($"Trying to authorize with clientId: {Options.ClientId} and clientSecret: {Options.ClientSecret}");
                string response =
                    await httpClient.GetStringAsync(
                        $"api/oauth.v2.access?code={code}&client_id={Options.ClientId}&client_secret={Options.ClientSecret}");
                Console.WriteLine($"Response after authorization: {response}");
                logger.LogInformation($"Response after authorization: {response}");
                var oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(response);
                return oauthResponse;
            }
        }

        public async Task<string[]> GetMembersOfConversations(string[] conversationIds, string appToken)
        {
            using (var httpClient = httpClientFactory.CreateClient(nameof(ISlackClient)))
            {
                var usersIds = new List<string>();
                foreach (var conversationId in conversationIds)
                {
                    httpClient.BaseAddress = new Uri("https://slack.com");
                    var keyValues = new List<KeyValuePair<string, string>>();
                    keyValues.Add(new KeyValuePair<string, string>("token", appToken));
                    keyValues.Add(new KeyValuePair<string, string>("channel", conversationId));

                    var values = new FormUrlEncodedContent(keyValues);
                    var response = await httpClient.PostAsync("api/conversations.members", values);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var r = JsonConvert.DeserializeObject<ListUserIdsResponse>(responseBody);
                        usersIds.AddRange(r.SlackUserIds);
                    }
                    else
                    {
                        logger.LogError($"Error during fetch slack profile, {response.StatusCode}");
                        return null;
                    }
                }

                return usersIds.ToArray();
            }
        }
    }
}
