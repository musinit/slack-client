﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Slack.Client.Models;
using Slack.Client.Models.SlackApi;

namespace Slack.Client
{
    public interface ISlackClient
    {
        /// <summary>
        /// Simple sending message in Slack. Send in test channel, if isTest = true
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isTest"></param>
        /// <returns></returns>
        Task<bool> SendMessageAsync(SendMessageRequest request, bool isTest = false);
        
        /// <summary>
        /// Send response message after simple message (can replace first message by itself)
        /// </summary>
        /// <param name="responseUrl"></param>
        /// <param name="blocks"></param>
        /// <returns></returns>
        Task SendResponseMessageAsync(string responseUrl, Block[] blocks);
        
        Task<SlackResponse> UpdateViewAsync<T>(UpdateViewRequest<T> request, string botToken, string requestUrl = "views.publish");
        
        /// <summary>
        /// Get user by his email
        /// </summary>
        /// <param name="email">User's email</param>
        /// <returns></returns>
        Task<SlackUser> GetUserIfExistAsync(string appToken, string email);
        
        /// <summary>
        /// Get user profile by his slackId
        /// </summary>
        /// <param name="appToken"></param>
        /// <param name="slackId"></param>
        /// <returns></returns>
        Task<Profile> GetUserProfile(string appToken, string slackId);

        /// <summary>
        /// Get list of user profiles
        /// </summary>
        /// <param name="appToken"></param>
        /// <param name="slackIds"></param>
        /// <returns></returns>
        Task<Profile[]> GetUserProfiles(string appToken, string[] slackIds);

        /// <summary>
        /// Get all users in workplace
        /// </summary>
        /// <param name="botToken"></param>
        /// <returns></returns>
        Task<SlackUser[]> GetAllUsers(string botToken);

        /// <summary>
        /// Get user info and hit team info after authorization (adding bot in workplace)
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        Task<OAuthResponse> Authorize(string code);
        
        /// <summary>
        /// Get members of conversation
        /// </summary>
        /// <param name="conversationIds"></param>
        /// <param name="appToken"></param>
        /// <returns></returns>
        Task<string[]> GetMembersOfConversations(string[] conversationIds, string appToken);
    }
}
